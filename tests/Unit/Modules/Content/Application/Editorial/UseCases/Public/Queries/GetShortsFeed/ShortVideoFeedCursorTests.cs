using _116.Content.Application.Editorial.UseCases.Public.Queries.GetShortsFeed;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetShortsFeed;

/// <summary>
/// Unit tests for <see cref="ShortVideoFeedCursor"/> encode/decode round-tripping and the
/// rejection of malformed tokens.
/// </summary>
public class ShortVideoFeedCursorTests
{
    [Fact]
    public void Encode_ThenTryDecode_ShouldRoundTripAllComponents()
    {
        // Arrange — negative AfterKey exercises the full signed 64-bit range
        var cursor = new ShortVideoFeedCursor(Seed: 987654L, AfterKey: -1234567890123L);

        // Act
        string token = cursor.Encode();
        bool decoded = ShortVideoFeedCursor.TryDecode(token, out ShortVideoFeedCursor result);

        // Assert
        decoded.Should().BeTrue();
        result.Seed.Should().Be(987654L);
        result.AfterKey.Should().Be(-1234567890123L);
    }

    [Fact]
    public void Encode_ThenTryDecode_WithSingleCharPadding_ShouldRoundTrip()
    {
        // "12|34" encodes to a base64url token whose length is 3 mod 4, exercising the
        // single-'=' re-padding branch on decode.
        var cursor = new ShortVideoFeedCursor(Seed: 12, AfterKey: 34);

        // Act
        string token = cursor.Encode();
        bool decoded = ShortVideoFeedCursor.TryDecode(token, out ShortVideoFeedCursor result);

        // Assert
        decoded.Should().BeTrue();
        result.Seed.Should().Be(12);
        result.AfterKey.Should().Be(34);
    }

    [Fact]
    public void Encode_ShouldProduceUrlSafeToken()
    {
        // Arrange
        var cursor = new ShortVideoFeedCursor(long.MaxValue, long.MinValue);

        // Act
        string token = cursor.Encode();

        // Assert
        token.Should().NotContain("+").And.NotContain("/").And.NotContain("=");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TryDecode_WhenNullOrBlank_ShouldReturnFalse(string? token)
    {
        // Act
        bool decoded = ShortVideoFeedCursor.TryDecode(token, out _);

        // Assert
        decoded.Should().BeFalse();
    }

    [Fact]
    public void TryDecode_WhenNotBase64_ShouldReturnFalse()
    {
        // Act
        bool decoded = ShortVideoFeedCursor.TryDecode("!!!not-a-valid-token!!!", out _);

        // Assert
        decoded.Should().BeFalse();
    }

    [Fact]
    public void TryDecode_WhenWrongComponentCount_ShouldReturnFalse()
    {
        // Arrange — base64url of "42|100|200" (three parts instead of two)
        string token = ToBase64Url("42|100|200");

        // Act
        bool decoded = ShortVideoFeedCursor.TryDecode(token, out _);

        // Assert
        decoded.Should().BeFalse();
    }

    [Fact]
    public void TryDecode_WhenSeedNotInteger_ShouldReturnFalse()
    {
        // Arrange
        string token = ToBase64Url("notanint|100");

        // Act
        bool decoded = ShortVideoFeedCursor.TryDecode(token, out _);

        // Assert
        decoded.Should().BeFalse();
    }

    [Fact]
    public void TryDecode_WhenAfterKeyNotInteger_ShouldReturnFalse()
    {
        // Arrange
        string token = ToBase64Url("42|not-a-number");

        // Act
        bool decoded = ShortVideoFeedCursor.TryDecode(token, out _);

        // Assert
        decoded.Should().BeFalse();
    }

    private static string ToBase64Url(string raw)
    {
        return Convert
            .ToBase64String(System.Text.Encoding.UTF8.GetBytes(raw))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
