using _116.Storage.Application.Ports;
using _116.Storage.Infrastructure.Services;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using SkiaSharp;
using Xunit;

namespace _116.Storage.Unit.Tests.Infrastructure.Services;

/// <summary>
/// Unit tests for <see cref="ImageColorService"/> covering dominant-color
/// extraction and the best-effort guarantees (no throw on bad input).
/// </summary>
public class ImageColorServiceTests
{
    private readonly ImageColorService _service = new();

    /// <summary>
    /// Encodes a solid-color image as PNG bytes for use as upload content.
    /// </summary>
    private static byte[] SolidColorPng(SKColor color, int width = 32, int height = 32)
    {
        var pixels = new SKColor[width * height];
        Array.Fill(pixels, color);
        return EncodePng(pixels, width, height);
    }

    /// <summary>
    /// Encodes a fully transparent image as PNG bytes.
    /// </summary>
    private static byte[] TransparentPng(int width = 32, int height = 32) =>
        EncodePng(new SKColor[width * height], width, height);

    /// <summary>
    /// Encodes a pixel buffer as PNG bytes.
    /// </summary>
    private static byte[] EncodePng(SKColor[] pixels, int width, int height)
    {
        using var bitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Unpremul));
        bitmap.Pixels = pixels;

        using SKData data = bitmap.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    private static IFormFile ImageFile(byte[] content) =>
        FileTestHelpers.CreateFormFileWithContent(content, "poster.png", "image/png");

    [Fact]
    public async Task ExtractAsync_WithSolidYellowImage_ShouldReturnYellowBackgroundAndBlackForeground()
    {
        IFormFile file = ImageFile(SolidColorPng(new SKColor(255, 235, 59)));

        ImageColors? colors = await _service.ExtractAsync(file);

        colors.Should().NotBeNull();
        colors!.DominantColorHex.Should().Be("#FFEB3B");
        colors.ForegroundColorHex.Should().Be("#000000");
    }

    [Fact]
    public async Task ExtractAsync_WithSolidNavyImage_ShouldReturnNavyBackgroundAndWhiteForeground()
    {
        IFormFile file = ImageFile(SolidColorPng(new SKColor(13, 27, 42)));

        ImageColors? colors = await _service.ExtractAsync(file);

        colors.Should().NotBeNull();
        colors!.DominantColorHex.Should().Be("#0D1B2A");
        colors.ForegroundColorHex.Should().Be("#FFFFFF");
    }

    [Fact]
    public async Task ExtractAsync_WithSolidWhiteImage_ShouldReturnBlackForeground()
    {
        IFormFile file = ImageFile(SolidColorPng(new SKColor(255, 255, 255)));

        ImageColors? colors = await _service.ExtractAsync(file);

        colors.Should().NotBeNull();
        colors!.DominantColorHex.Should().Be("#FFFFFF");
        colors.ForegroundColorHex.Should().Be("#000000");
    }

    [Fact]
    public async Task ExtractAsync_WhenImageDominatedByOneColor_ShouldReturnThatColor()
    {
        // 100x100 (matches the analysis size, so no resampling) mostly-red image with
        // a small 10x10 blue corner; red must win the histogram by an overwhelming
        // margin. The pixel buffer is built in a single pass — no nested loops.
        const int width = 100;
        const int height = 100;
        var red = new SKColor(200, 30, 30);
        var blue = new SKColor(30, 30, 200);

        SKColor[] pixels = Enumerable
            .Range(0, width * height)
            .Select(index => index % width < 10 && index / width < 10 ? blue : red)
            .ToArray();

        IFormFile file = ImageFile(EncodePng(pixels, width, height));

        ImageColors? colors = await _service.ExtractAsync(file);

        colors.Should().NotBeNull();
        colors!.DominantColorHex.Should().Be("#C81E1E");
    }

    [Fact]
    public async Task ExtractAsync_WithFullyTransparentImage_ShouldReturnNull()
    {
        IFormFile file = ImageFile(TransparentPng());

        ImageColors? colors = await _service.ExtractAsync(file);

        colors.Should().BeNull();
    }

    [Fact]
    public async Task ExtractAsync_WithNonImageContent_ShouldReturnNullAndNotThrow()
    {
        IFormFile file = FileTestHelpers.CreateFormFileWithContent(
            "this is not an image"u8.ToArray(),
            "notes.txt",
            "text/plain"
        );

        ImageColors? colors = await _service.ExtractAsync(file);

        colors.Should().BeNull();
    }
}
