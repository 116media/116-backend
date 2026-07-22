using _116.Content.Application.Shared;
using _116.Content.Domain.Enums;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Shared;

/// <summary>
/// Unit tests for <see cref="StreamingLinkResolver"/>.
/// </summary>
public class StreamingLinkResolverTests
{
    private const string ArtistName = "Fally Ipupa";
    private const string ReleaseName = "Eloko Oyo";

    #region ResolveStreamingLinks Tests

    [Fact]
    public void ResolveStreamingLinks_ShouldAlwaysReturnExactlyFourPlatforms()
    {
        // Arrange
        var curated = new Dictionary<EnumStreamingPlatform, string>();

        // Act
        IReadOnlyList<(EnumStreamingPlatform Platform, string Url)> result =
            StreamingLinkResolver.ResolveStreamingLinks(ArtistName, ReleaseName, curated);

        // Assert
        result.Should().HaveCount(4);
        result
            .Select(r => r.Platform)
            .Should()
            .BeEquivalentTo(
                new[]
                {
                    EnumStreamingPlatform.Spotify,
                    EnumStreamingPlatform.AppleMusic,
                    EnumStreamingPlatform.YoutubeMusic,
                    EnumStreamingPlatform.Tidal,
                }
            );
    }

    [Fact]
    public void ResolveStreamingLinks_WithNoCuratedLinks_ShouldReturnValidGeneratedFallbackForEveryPlatform()
    {
        // Arrange
        var curated = new Dictionary<EnumStreamingPlatform, string>();

        // Act
        IReadOnlyList<(EnumStreamingPlatform Platform, string Url)> result =
            StreamingLinkResolver.ResolveStreamingLinks(ArtistName, ReleaseName, curated);

        // Assert
        result.Should().OnlyContain(r => !string.IsNullOrWhiteSpace(r.Url));
        result
            .Single(r => r.Platform == EnumStreamingPlatform.Spotify)
            .Url.Should()
            .StartWith("https://open.spotify.com/search/");
        result
            .Single(r => r.Platform == EnumStreamingPlatform.AppleMusic)
            .Url.Should()
            .StartWith("https://music.apple.com/search?term=");
        result
            .Single(r => r.Platform == EnumStreamingPlatform.YoutubeMusic)
            .Url.Should()
            .StartWith("https://music.youtube.com/search?q=");
        result
            .Single(r => r.Platform == EnumStreamingPlatform.Tidal)
            .Url.Should()
            .StartWith("https://listen.tidal.com/search?q=");
    }

    [Fact]
    public void ResolveStreamingLinks_WhenCuratedLinkExistsForPlatform_ShouldPreferCuratedOverGenerated()
    {
        // Arrange
        const string curatedUrl = "https://open.spotify.com/album/curated-abc123";
        var curated = new Dictionary<EnumStreamingPlatform, string> { [EnumStreamingPlatform.Spotify] = curatedUrl };

        // Act
        IReadOnlyList<(EnumStreamingPlatform Platform, string Url)> result =
            StreamingLinkResolver.ResolveStreamingLinks(ArtistName, ReleaseName, curated);

        // Assert
        result.Single(r => r.Platform == EnumStreamingPlatform.Spotify).Url.Should().Be(curatedUrl);
    }

    [Fact]
    public void ResolveStreamingLinks_WhenCuratedLinkMissingForPlatform_ShouldFallBackToGeneratedUrlForThatPlatformOnly()
    {
        // Arrange — only Spotify has a curated link; every other platform must fall back.
        const string curatedUrl = "https://open.spotify.com/album/curated-abc123";
        var curated = new Dictionary<EnumStreamingPlatform, string> { [EnumStreamingPlatform.Spotify] = curatedUrl };

        // Act
        IReadOnlyList<(EnumStreamingPlatform Platform, string Url)> result =
            StreamingLinkResolver.ResolveStreamingLinks(ArtistName, ReleaseName, curated);

        // Assert
        result.Single(r => r.Platform == EnumStreamingPlatform.Spotify).Url.Should().Be(curatedUrl);
        result
            .Single(r => r.Platform == EnumStreamingPlatform.AppleMusic)
            .Url.Should()
            .StartWith("https://music.apple.com/search?term=");
        result
            .Single(r => r.Platform == EnumStreamingPlatform.YoutubeMusic)
            .Url.Should()
            .StartWith("https://music.youtube.com/search?q=");
        result
            .Single(r => r.Platform == EnumStreamingPlatform.Tidal)
            .Url.Should()
            .StartWith("https://listen.tidal.com/search?q=");
    }

    [Fact]
    public void ResolveStreamingLinks_WhenAllPlatformsCurated_ShouldReturnAllCuratedUrlsVerbatim()
    {
        // Arrange
        var curated = new Dictionary<EnumStreamingPlatform, string>
        {
            [EnumStreamingPlatform.Spotify] = "https://open.spotify.com/album/1",
            [EnumStreamingPlatform.AppleMusic] = "https://music.apple.com/album/2",
            [EnumStreamingPlatform.YoutubeMusic] = "https://music.youtube.com/playlist/3",
            [EnumStreamingPlatform.Tidal] = "https://listen.tidal.com/album/4",
        };

        // Act
        IReadOnlyList<(EnumStreamingPlatform Platform, string Url)> result =
            StreamingLinkResolver.ResolveStreamingLinks(ArtistName, ReleaseName, curated);

        // Assert
        foreach ((EnumStreamingPlatform platform, string url) in result)
        {
            url.Should().Be(curated[platform]);
        }
    }

    #endregion
}
