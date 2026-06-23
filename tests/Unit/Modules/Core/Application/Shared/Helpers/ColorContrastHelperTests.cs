using _116.Core.Application.Shared.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Core.Application.Shared.Helpers;

/// <summary>
/// Unit tests for <see cref="ColorContrastHelper"/> covering hex normalization and
/// the WCAG luminance-based foreground (text) color selection.
/// </summary>
public class ColorContrastHelperTests
{
    #region Normalize Tests

    [Theory]
    [InlineData("#FFEB3B", "#FFEB3B")]
    [InlineData("ffeb3b", "#FFEB3B")]
    [InlineData("#ffeb3b", "#FFEB3B")]
    [InlineData("  #0d1b2a  ", "#0D1B2A")]
    [InlineData("000000", "#000000")]
    public void Normalize_WithValidHex_ShouldReturnCanonicalUpperCase(string input, string expected)
    {
        string? result = ColorContrastHelper.Normalize(input);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("#FFF")]
    [InlineData("FFEB3")]
    [InlineData("#FFEB3BB")]
    [InlineData("#GGGGGG")]
    [InlineData("not-a-color")]
    public void Normalize_WithInvalidHex_ShouldReturnNull(string? input)
    {
        string? result = ColorContrastHelper.Normalize(input);

        result.Should().BeNull();
    }

    #endregion

    #region ForegroundFor Tests

    [Theory]
    [InlineData("#FFFFFF")] // white
    [InlineData("#FFEB3B")] // yellow — the canonical "light bg, must use black text" case
    [InlineData("#FF9800")] // orange
    [InlineData("#00FF00")] // bright green (high luminance)
    [InlineData("#FF0000")] // pure red (luminance ~0.2126, just above the threshold)
    public void ForegroundFor_WithLightBackground_ShouldReturnBlack(string background)
    {
        string? foreground = ColorContrastHelper.ForegroundFor(background);

        foreground.Should().Be(ColorContrastHelper.Black);
    }

    [Theory]
    [InlineData("#000000")] // black
    [InlineData("#0D1B2A")] // navy
    [InlineData("#0000FF")] // pure blue (very low luminance)
    [InlineData("#4A148C")] // deep purple
    public void ForegroundFor_WithDarkBackground_ShouldReturnWhite(string background)
    {
        string? foreground = ColorContrastHelper.ForegroundFor(background);

        foreground.Should().Be(ColorContrastHelper.White);
    }

    [Fact]
    public void ForegroundFor_ShouldAcceptInputWithoutLeadingHashAndLowerCase()
    {
        string? foreground = ColorContrastHelper.ForegroundFor("ffeb3b");

        foreground.Should().Be(ColorContrastHelper.Black);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("#FFF")]
    [InlineData("#ZZZZZZ")]
    public void ForegroundFor_WithInvalidBackground_ShouldReturnNull(string? background)
    {
        string? foreground = ColorContrastHelper.ForegroundFor(background);

        foreground.Should().BeNull();
    }

    #endregion
}
