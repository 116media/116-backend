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
        // Arrange
        var afterId = Guid.NewGuid();
        var cursor = new ShortVideoFeedCursor(Seed: 987654, AfterSortKey: "a1b2c3d4e5f6", AfterId: afterId);

        // Act
        string token = cursor.Encode();
        bool decoded = ShortVideoFeedCursor.TryDecode(token, out ShortVideoFeedCursor result);

        // Assert
        decoded.Should().BeTrue();
        result.Seed.Should().Be(987654);
        result.AfterSortKey.Should().Be("a1b2c3d4e5f6");
        result.AfterId.Should().Be(afterId);
    }

    [Fact]
    public void Encode_ShouldProduceUrlSafeToken()
    {
        // Arrange
        var cursor = new ShortVideoFeedCursor(int.MaxValue, "ffffffffffffffffffffffffffffffff", Guid.NewGuid());

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
        // Arrange — base64url of "42|onlytwo"
        string token = ToBase64Url("42|onlytwo");

        // Act
        bool decoded = ShortVideoFeedCursor.TryDecode(token, out _);

        // Assert
        decoded.Should().BeFalse();
    }

    [Fact]
    public void TryDecode_WhenSeedNotInteger_ShouldReturnFalse()
    {
        // Arrange
        string token = ToBase64Url($"notanint|sortkey|{Guid.NewGuid()}");

        // Act
        bool decoded = ShortVideoFeedCursor.TryDecode(token, out _);

        // Assert
        decoded.Should().BeFalse();
    }

    [Fact]
    public void TryDecode_WhenAfterIdNotGuid_ShouldReturnFalse()
    {
        // Arrange
        string token = ToBase64Url("42|sortkey|not-a-guid");

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
