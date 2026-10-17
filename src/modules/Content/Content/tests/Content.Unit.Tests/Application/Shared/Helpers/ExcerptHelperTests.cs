using _116.Content.Application.Shared.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Shared.Helpers;

/// <summary>
/// Unit tests for <see cref="ExcerptHelper" />.
/// </summary>
public class ExcerptHelperTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Truncate_WithNoText_ShouldReturnEmpty(string? text)
    {
        // Act
        string result = ExcerptHelper.Truncate(text);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Truncate_WithShortText_ShouldReturnItTrimmed()
    {
        // Act
        string result = ExcerptHelper.Truncate("  une phrase courte  ");

        // Assert
        result.Should().Be("une phrase courte");
    }

    [Fact]
    public void Truncate_WithLongText_ShouldNotSplitAWord()
    {
        // Arrange
        string text = string.Join(' ', Enumerable.Repeat("rumba", 100));

        // Act
        string result = ExcerptHelper.Truncate(text);

        // Assert
        result.Length.Should().BeLessThanOrEqualTo(200);
        result.Should().NotEndWith("rumb");
        text.Should().StartWith(result);
    }

    [Fact]
    public void Truncate_WithAnUnbrokenRun_ShouldCutAtTheCeiling()
    {
        // Arrange
        string text = new('a', 250);

        // Act
        string result = ExcerptHelper.Truncate(text);

        // Assert
        result.Length.Should().Be(200);
    }
}
