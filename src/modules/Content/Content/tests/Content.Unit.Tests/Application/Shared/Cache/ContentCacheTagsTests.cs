using _116.Content.Application.Shared.Cache;
using _116.Content.Domain.Enums;
using AwesomeAssertions;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Shared.Cache;

/// <summary>
/// Unit tests for <see cref="ContentCacheTags" />. The mapping decides which cached feeds a
/// publish evicts, so a type silently falling through to the lookup tag is a stale-feed bug.
/// </summary>
public class ContentCacheTagsTests
{
    [Theory]
    [InlineData(EnumCoreContentType.Article, ContentCacheTags.Articles)]
    [InlineData(EnumCoreContentType.Video, ContentCacheTags.Videos)]
    [InlineData(EnumCoreContentType.Short, ContentCacheTags.Shorts)]
    [InlineData(EnumCoreContentType.Lyrics, ContentCacheTags.Lyrics)]
    public void For_WithATypeThatOwnsATag_ShouldReturnIt(EnumCoreContentType type, string expected)
    {
        // Act
        string tag = ContentCacheTags.For(type);

        // Assert
        tag.Should().Be(expected);
    }

    [Fact]
    public void For_WithATypeThatOwnsNoTag_ShouldFallBackToTheLookupTag()
    {
        // Act
        string tag = ContentCacheTags.For(EnumCoreContentType.Custom);

        // Assert
        tag.Should().Be(ContentCacheTags.Lookups);
    }

    [Fact]
    public void For_ShouldAnswerForEveryDeclaredType()
    {
        // Arrange
        EnumCoreContentType[] types = Enum.GetValues<EnumCoreContentType>();

        // Act
        IEnumerable<string> tags = types.Select(ContentCacheTags.For);

        // Assert
        tags.Should().AllSatisfy(tag => tag.Should().StartWith("content:"));
    }
}
