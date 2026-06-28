using _116.Core.Application.Shared.Services;
using _116.Core.Infrastructure.Services;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace _116.Unit.Tests.Modules.Core.Infrastructure.Services;

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
    private static byte[] SolidColorPng(Rgba32 color, int width = 32, int height = 32)
    {
        using var image = new Image<Rgba32>(width, height, color);
        using var stream = new MemoryStream();
        image.SaveAsPng(stream);
        return stream.ToArray();
    }

    /// <summary>
    /// Encodes a fully transparent image as PNG bytes.
    /// </summary>
    private static byte[] TransparentPng(int width = 32, int height = 32)
    {
        using var image = new Image<Rgba32>(width, height, new Rgba32(0, 0, 0, 0));
        using var stream = new MemoryStream();
        image.SaveAsPng(stream);
        return stream.ToArray();
    }

    private static IFormFile ImageFile(byte[] content) =>
        FileTestHelpers.CreateFormFileWithContent(content, "poster.png", "image/png");

    [Fact]
    public async Task ExtractAsync_WithSolidYellowImage_ShouldReturnYellowBackgroundAndBlackForeground()
    {
        IFormFile file = ImageFile(SolidColorPng(new Rgba32(255, 235, 59)));

        ImageColors? colors = await _service.ExtractAsync(file);

        colors.Should().NotBeNull();
        colors!.DominantColorHex.Should().Be("#FFEB3B");
        colors.ForegroundColorHex.Should().Be("#000000");
    }

    [Fact]
    public async Task ExtractAsync_WithSolidNavyImage_ShouldReturnNavyBackgroundAndWhiteForeground()
    {
        IFormFile file = ImageFile(SolidColorPng(new Rgba32(13, 27, 42)));

        ImageColors? colors = await _service.ExtractAsync(file);

        colors.Should().NotBeNull();
        colors!.DominantColorHex.Should().Be("#0D1B2A");
        colors.ForegroundColorHex.Should().Be("#FFFFFF");
    }

    [Fact]
    public async Task ExtractAsync_WithSolidWhiteImage_ShouldReturnBlackForeground()
    {
        IFormFile file = ImageFile(SolidColorPng(new Rgba32(255, 255, 255)));

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
        var red = new Rgba32(200, 30, 30);
        var blue = new Rgba32(30, 30, 200);

        Rgba32[] pixels = Enumerable
            .Range(0, width * height)
            .Select(index => index % width < 10 && index / width < 10 ? blue : red)
            .ToArray();

        using var image = Image.LoadPixelData(pixels, width, height);

        using var stream = new MemoryStream();
        image.SaveAsPng(stream);
        IFormFile file = ImageFile(stream.ToArray());

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
