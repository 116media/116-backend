using _116.Content.Infrastructure.Services;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Infrastructure.Services;

/// <summary>
/// Unit tests for <see cref="PlaceholderTranslationService"/>.
/// </summary>
public class PlaceholderTranslationServiceTests
{
    private readonly PlaceholderTranslationService _service = new();

    #region TranslateAsync Tests

    [Fact]
    public async Task TranslateAsync_ShouldReturnTheSourceTextUnchanged()
    {
        // Arrange
        const string text = "Nakei kobina na ndenge na yo.";

        // Act
        string result = await _service.TranslateAsync(text, "es");

        // Assert
        result.Should().Be(text);
    }

    [Theory]
    [InlineData("es")]
    [InlineData("fr")]
    [InlineData("en")]
    public async Task TranslateAsync_ShouldIgnoreTheTargetLanguage(string targetLanguage)
    {
        // Arrange
        const string text = "Multi-line\nlyrics body.";

        // Act
        string result = await _service.TranslateAsync(text, targetLanguage);

        // Assert
        result.Should().Be(text);
    }

    [Fact]
    public async Task TranslateAsync_WithEmptyText_ShouldReturnEmptyText()
    {
        // Act
        string result = await _service.TranslateAsync(string.Empty, "fr");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task TranslateAsync_WithCancellationToken_ShouldStillReturnTheSourceText()
    {
        // Arrange
        const string text = "Source text.";
        using var cancellation = new CancellationTokenSource();

        // Act
        string result = await _service.TranslateAsync(text, "fr", cancellation.Token);

        // Assert
        result.Should().Be(text);
    }

    #endregion
}
