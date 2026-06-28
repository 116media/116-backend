using _116.Core.Application.Shared.Helpers;
using _116.Core.Application.Shared.Services;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace _116.Core.Infrastructure.Services;

/// <summary>
/// Extracts the dominant color from an image with SixLabors.ImageSharp and
/// derives a contrasting foreground via <see cref="ColorContrastHelper" />.
/// </summary>
/// <remarks>
/// The image is downscaled and its pixels are bucketed into a coarse color
/// histogram; the most-populated bucket's average becomes the dominant color.
/// Averaging the bucket (rather than snapping to the bucket center) keeps the
/// result faithful to the artwork. Decoding/analysis is fully guarded so a
/// malformed or non-image file never throws into the upload pipeline.
/// </remarks>
public class ImageColorService : IImageColorService
{
    /// <summary>
    /// Longest edge (px) the image is downscaled to before analysis. Small enough
    /// to stay fast on large posters, large enough to keep the dominant hue stable.
    /// </summary>
    private const int MaxAnalyzedDimension = 100;

    /// <summary>
    /// Number of low bits dropped per channel when bucketing colors. A shift of 4
    /// yields 16 levels per channel (4096 buckets), grouping near-identical colors
    /// while keeping distinct hues apart.
    /// </summary>
    private const int ChannelShift = 4;

    /// <summary>
    /// Bits retained per channel after bucketing (<c>8 - ChannelShift</c>). Used to
    /// pack the three channels into a single histogram index.
    /// </summary>
    private const int LevelBits = 8 - ChannelShift;

    /// <summary>
    /// Total number of histogram buckets (<c>2 ^ (3 * LevelBits)</c>). The key space
    /// is small and gap-free, so a flat array indexes it directly — no hashing.
    /// </summary>
    private const int BucketCount = 1 << (3 * LevelBits);

    /// <summary>
    /// Minimum alpha for a pixel to count. Near-transparent pixels carry no visible
    /// color and are skipped so they cannot win the histogram.
    /// </summary>
    private const byte MinOpaqueAlpha = 16;

    /// <inheritdoc />
    public async Task<ImageColors?> ExtractAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        try
        {
            await using Stream stream = file.OpenReadStream();
            using Image<Rgba32> image = await Image.LoadAsync<Rgba32>(stream, cancellationToken);

            image.Mutate(context =>
                context.Resize(
                    new ResizeOptions
                    {
                        Sampler = KnownResamplers.Box,
                        Size = new Size(MaxAnalyzedDimension, MaxAnalyzedDimension),
                        Mode = ResizeMode.Max,
                    }
                )
            );

            string? dominant = FindDominantColor(image);
            if (dominant is null)
            {
                return null;
            }

            string? foreground = ColorContrastHelper.ForegroundFor(dominant);
            return foreground is null ? null : new ImageColors(dominant, foreground);
        }
        catch (Exception)
        {
            // Color extraction is best-effort and must never block the upload.
            return null;
        }
    }

    /// <summary>
    /// Builds a coarse color histogram over the image's opaque pixels and returns
    /// the average color of the most-populated bucket as <c>#RRGGBB</c>.
    /// </summary>
    /// <param name="image">The (already downscaled) image to analyze.</param>
    /// <returns>The dominant color, or <c>null</c> when no opaque pixels exist.</returns>
    private static string? FindDominantColor(Image<Rgba32> image)
    {
        var pixels = new Rgba32[image.Width * image.Height];
        image.CopyPixelDataTo(pixels);

        var buckets = new ColorAccumulator[BucketCount];

        foreach (ref readonly Rgba32 pixel in pixels.AsSpan())
        {
            if (pixel.A < MinOpaqueAlpha)
            {
                continue;
            }

            int key =
                ((pixel.R >> ChannelShift) << (2 * LevelBits))
                | ((pixel.G >> ChannelShift) << LevelBits)
                | (pixel.B >> ChannelShift);

            buckets[key] = buckets[key].Add(pixel.R, pixel.G, pixel.B);
        }

        ColorAccumulator dominant = buckets.MaxBy(bucket => bucket.Count);

        // Count is zero only when every pixel was transparent (no bucket filled).
        return dominant.Count == 0 ? null : dominant.ToHex();
    }

    /// <summary>
    /// Running per-bucket totals used to compute a bucket's average color without
    /// retaining individual pixels.
    /// </summary>
    /// <param name="Count">Number of pixels accumulated into the bucket.</param>
    /// <param name="SumR">Sum of the red channel across accumulated pixels.</param>
    /// <param name="SumG">Sum of the green channel across accumulated pixels.</param>
    /// <param name="SumB">Sum of the blue channel across accumulated pixels.</param>
    private readonly record struct ColorAccumulator(long Count, long SumR, long SumG, long SumB)
    {
        /// <summary>
        /// Returns a new accumulator with the given pixel folded in.
        /// </summary>
        /// <param name="r">The pixel's red channel.</param>
        /// <param name="g">The pixel's green channel.</param>
        /// <param name="b">The pixel's blue channel.</param>
        /// <returns>The updated accumulator.</returns>
        public ColorAccumulator Add(byte r, byte g, byte b) => new(Count + 1, SumR + r, SumG + g, SumB + b);

        /// <summary>
        /// Computes this bucket's average color as canonical upper-case <c>#RRGGBB</c>.
        /// </summary>
        /// <returns>The average color of the accumulated pixels.</returns>
        public string ToHex()
        {
            int r = (int)(SumR / Count);
            int g = (int)(SumG / Count);
            int b = (int)(SumB / Count);
            return $"#{r:X2}{g:X2}{b:X2}";
        }
    }
}
