using System.Net;
using _116.Content.Application.Editorial.Factories;
using _116.Content.Domain.Enums;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.Factories;

/// <summary>
/// Unit tests for <see cref="StreamingLinkFactory"/>.
/// </summary>
public class StreamingLinkFactoryTests
{
    private const string ArtistName = "Fally Ipupa";
    private const string ReleaseName = "Eloko Oyo";

    #region CreateStreamingLinks Tests

    [Fact]
    public void CreateStreamingLinks_ShouldAlwaysReturnExactlyFivePlatforms()
    {
        // Arrange
        var curated = new Dictionary<EnumStreamingPlatform, string>();

        // Act
        IReadOnlyList<(EnumStreamingPlatform Platform, string Url)> result = StreamingLinkFactory.CreateStreamingLinks(
            ArtistName,
            ReleaseName,
            curated
        );

        // Assert
        result.Should().HaveCount(5);
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
                    EnumStreamingPlatform.Deezer,
                }
            );
    }

    [Fact]
    public void CreateStreamingLinks_WithNoCuratedLinks_ShouldReturnValidGeneratedFallbackForEveryPlatform()
    {
        // Arrange
        var curated = new Dictionary<EnumStreamingPlatform, string>();

        // Act
        IReadOnlyList<(EnumStreamingPlatform Platform, string Url)> result = StreamingLinkFactory.CreateStreamingLinks(
            ArtistName,
            ReleaseName,
            curated
        );

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
        result
            .Single(r => r.Platform == EnumStreamingPlatform.Deezer)
            .Url.Should()
            .StartWith("https://www.deezer.com/search/");
    }

    [Fact]
    public void CreateStreamingLinks_WhenCuratedLinkExistsForPlatform_ShouldPreferCuratedOverGenerated()
    {
        // Arrange
        const string curatedUrl = "https://open.spotify.com/album/curated-abc123";
        var curated = new Dictionary<EnumStreamingPlatform, string> { [EnumStreamingPlatform.Spotify] = curatedUrl };

        // Act
        IReadOnlyList<(EnumStreamingPlatform Platform, string Url)> result = StreamingLinkFactory.CreateStreamingLinks(
            ArtistName,
            ReleaseName,
            curated
        );

        // Assert
        result.Single(r => r.Platform == EnumStreamingPlatform.Spotify).Url.Should().Be(curatedUrl);
    }

    [Fact]
    public void CreateStreamingLinks_WhenCuratedLinkMissingForPlatform_ShouldFallBackToGeneratedUrlForThatPlatformOnly()
    {
        // Arrange — only Spotify has a curated link; every other platform must fall back.
        const string curatedUrl = "https://open.spotify.com/album/curated-abc123";
        var curated = new Dictionary<EnumStreamingPlatform, string> { [EnumStreamingPlatform.Spotify] = curatedUrl };

        // Act
        IReadOnlyList<(EnumStreamingPlatform Platform, string Url)> result = StreamingLinkFactory.CreateStreamingLinks(
            ArtistName,
            ReleaseName,
            curated
        );

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
        result
            .Single(r => r.Platform == EnumStreamingPlatform.Deezer)
            .Url.Should()
            .StartWith("https://www.deezer.com/search/");
    }

    [Fact]
    public void CreateStreamingLinks_WhenAllPlatformsCurated_ShouldReturnAllCuratedUrlsVerbatim()
    {
        // Arrange
        var curated = new Dictionary<EnumStreamingPlatform, string>
        {
            [EnumStreamingPlatform.Spotify] = "https://open.spotify.com/album/1",
            [EnumStreamingPlatform.AppleMusic] = "https://music.apple.com/album/2",
            [EnumStreamingPlatform.YoutubeMusic] = "https://music.youtube.com/playlist/3",
            [EnumStreamingPlatform.Tidal] = "https://listen.tidal.com/album/4",
            [EnumStreamingPlatform.Deezer] = "https://www.deezer.com/album/5",
        };

        // Act
        IReadOnlyList<(EnumStreamingPlatform Platform, string Url)> result = StreamingLinkFactory.CreateStreamingLinks(
            ArtistName,
            ReleaseName,
            curated
        );

        // Assert
        foreach ((EnumStreamingPlatform platform, string url) in result)
        {
            url.Should().Be(curated[platform]);
        }
    }

    [Fact]
    public void CreateStreamingLinks_ShouldUrlEncodeTheArtistAndReleaseInTheGeneratedSearchQuery()
    {
        // Arrange — names containing characters that must be escaped in a query string.
        const string artistName = "Koffi & Quartier";
        const string releaseName = "Effrakata / Mopao";
        var curated = new Dictionary<EnumStreamingPlatform, string>();

        // Act
        IReadOnlyList<(EnumStreamingPlatform Platform, string Url)> result = StreamingLinkFactory.CreateStreamingLinks(
            artistName,
            releaseName,
            curated
        );

        // Assert
        string encoded = WebUtility.UrlEncode($"{artistName} {releaseName}");
        result.Should().OnlyContain(r => r.Url.EndsWith(encoded, StringComparison.Ordinal));
        result.Should().OnlyContain(r => !r.Url.Contains(' '));
    }

    #endregion

    #region GenerateSearchUrl guard

    [Fact]
    public void GenerateSearchUrl_WithUndeclaredPlatform_ShouldThrowArgumentOutOfRange()
    {
        // Arrange
        const EnumStreamingPlatform undeclared = (EnumStreamingPlatform)9999;

        // Act
        Action act = () => StreamingLinkFactory.GenerateSearchUrl(undeclared, "Fally Ipupa", "Eloko Oyo");

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("platform");
    }

    [Fact]
    public void GenerateSearchUrl_WithEveryDeclaredPlatform_ShouldReturnAUrl()
    {
        // Act
        IEnumerable<string> urls = Enum.GetValues<EnumStreamingPlatform>()
            .Select(p => StreamingLinkFactory.GenerateSearchUrl(p, "Fally Ipupa", "Eloko Oyo"));

        // Assert
        urls.Should().OnlyContain(url => url.StartsWith("https://", StringComparison.Ordinal));
    }

    #endregion
}
