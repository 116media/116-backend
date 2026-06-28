using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace _116.Tests.Fixtures.Helpers;

/// <summary>
/// Helpers for generating real, decodable image bytes in tests — used by code
/// paths that actually read and analyze the uploaded image (e.g. dominant-color
/// extraction), where a stub byte array would fail to decode.
/// </summary>
public static class ImageTestHelpers
{
    /// <summary>
    /// Generates a solid-color image encoded as PNG bytes.
    /// </summary>
    /// <param name="red">The red channel (0-255).</param>
    /// <param name="green">The green channel (0-255).</param>
    /// <param name="blue">The blue channel (0-255).</param>
    /// <param name="width">The image width in pixels.</param>
    /// <param name="height">The image height in pixels.</param>
    /// <returns>The PNG-encoded bytes of the solid-color image.</returns>
    public static byte[] SolidColorPng(byte red, byte green, byte blue, int width = 64, int height = 64)
    {
        using var image = new Image<Rgba32>(width, height, new Rgba32(red, green, blue));
        using var stream = new MemoryStream();
        image.SaveAsPng(stream);
        return stream.ToArray();
    }

    /// <summary>
    /// Generates a multi-color image that is overwhelmingly one color, with a
    /// small square accent of a second color in the top-left corner, encoded as
    /// PNG. The image genuinely contains several colors, yet the dominant color
    /// wins the histogram by a wide margin, so extraction is deterministic. The
    /// pixel buffer is built in a single pass — no nested loops.
    /// </summary>
    /// <param name="dominant">The color that fills most of the image (R, G, B).</param>
    /// <param name="accent">The color of the small corner block (R, G, B).</param>
    /// <param name="width">The image width in pixels.</param>
    /// <param name="height">The image height in pixels.</param>
    /// <param name="accentSize">The side length of the square accent block in pixels.</param>
    /// <returns>The PNG-encoded bytes of the dominated multi-color image.</returns>
    public static byte[] DominantColorPng(
        (byte R, byte G, byte B) dominant,
        (byte R, byte G, byte B) accent,
        int width = 100,
        int height = 100,
        int accentSize = 20
    )
    {
        var dominantPixel = new Rgba32(dominant.R, dominant.G, dominant.B);
        var accentPixel = new Rgba32(accent.R, accent.G, accent.B);

        Rgba32[] pixels = Enumerable
            .Range(0, width * height)
            .Select(index => index % width < accentSize && index / width < accentSize ? accentPixel : dominantPixel)
            .ToArray();

        using var image = Image.LoadPixelData<Rgba32>(pixels, width, height);
        using var stream = new MemoryStream();
        image.SaveAsPng(stream);
        return stream.ToArray();
    }
}
