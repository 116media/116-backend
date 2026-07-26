using System.Net;
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

    [Fact]
    public void ResolveStreamingLinks_ShouldUrlEncodeTheArtistAndReleaseInTheGeneratedSearchQuery()
    {
        // Arrange — names containing characters that must be escaped in a query string.
        const string artistName = "Koffi & Quartier";
        const string releaseName = "Effrakata / Mopao";
        var curated = new Dictionary<EnumStreamingPlatform, string>();

        // Act
        IReadOnlyList<(EnumStreamingPlatform Platform, string Url)> result =
            StreamingLinkResolver.ResolveStreamingLinks(artistName, releaseName, curated);

        // Assert
        string encoded = WebUtility.UrlEncode($"{artistName} {releaseName}");
        result.Should().OnlyContain(r => r.Url.EndsWith(encoded, StringComparison.Ordinal));
        result.Should().OnlyContain(r => !r.Url.Contains(' '));
    }

    #endregion

    #region GenerateSearchUrl guard

    /// <summary>
    /// The unsupported-platform guard is unreachable through <c>ResolveStreamingLinks</c>, which
    /// only iterates declared enum members. It exists so that adding a fifth platform without a
    /// matching switch arm fails loudly instead of silently returning a wrong URL, so it is
    /// asserted directly against an undeclared enum value.
    /// </summary>
    [Fact]
    public void GenerateSearchUrl_WithUndeclaredPlatform_ShouldThrowArgumentOutOfRange()
    {
        // Arrange
        const EnumStreamingPlatform undeclared = (EnumStreamingPlatform)9999;

        // Act
        Action act = () => StreamingLinkResolver.GenerateSearchUrl(undeclared, "Fally Ipupa", "Eloko Oyo");

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("platform");
    }

    /// <summary>
    /// Every declared platform has an explicit switch arm, so none of them hits the guard.
    /// </summary>
    [Fact]
    public void GenerateSearchUrl_WithEveryDeclaredPlatform_ShouldReturnAUrl()
    {
        // Act
        IEnumerable<string> urls = Enum.GetValues<EnumStreamingPlatform>()
            .Select(p => StreamingLinkResolver.GenerateSearchUrl(p, "Fally Ipupa", "Eloko Oyo"));

        // Assert
        urls.Should().OnlyContain(url => url.StartsWith("https://", StringComparison.Ordinal));
    }

    #endregion
}
