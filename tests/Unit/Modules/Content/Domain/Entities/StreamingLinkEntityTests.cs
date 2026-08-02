using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="StreamingLinkEntity"/>.
/// </summary>
public class StreamingLinkEntityTests
{
    private const string ValidUrl = "https://open.spotify.com/album/abc123";

    #region ForAlbum Tests

    [Fact]
    public void ForAlbum_WithValidParams_ShouldSetAlbumIdAndClearLyricsId()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();

        // Act
        StreamingLinkEntity link = StreamingLinkEntity.ForAlbum(id, albumId, EnumStreamingPlatform.Spotify, ValidUrl);

        // Assert
        link.Id.Should().Be(id);
        link.AlbumId.Should().Be(albumId);
        link.LyricsId.Should().BeNull();
        link.Platform.Should().Be(EnumStreamingPlatform.Spotify);
        link.Url.Should().Be(ValidUrl);
    }

    #endregion

    #region ForSingle Tests

    [Fact]
    public void ForSingle_WithValidParams_ShouldSetLyricsIdAndClearAlbumId()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Guid lyricsId = Guid.NewGuid();

        // Act
        StreamingLinkEntity link = StreamingLinkEntity.ForSingle(
            id,
            lyricsId,
            EnumStreamingPlatform.AppleMusic,
            ValidUrl
        );

        // Assert
        link.Id.Should().Be(id);
        link.LyricsId.Should().Be(lyricsId);
        link.AlbumId.Should().BeNull();
        link.Platform.Should().Be(EnumStreamingPlatform.AppleMusic);
        link.Url.Should().Be(ValidUrl);
    }

    #endregion

    #region UpdateUrl Tests

    [Fact]
    public void UpdateUrl_ShouldReplaceTheCuratedUrl()
    {
        // Arrange
        StreamingLinkEntity link = StreamingLinkEntity.ForAlbum(
            Guid.NewGuid(),
            Guid.NewGuid(),
            EnumStreamingPlatform.Tidal,
            ValidUrl
        );
        const string newUrl = "https://listen.tidal.com/album/456";

        // Act
        link.UpdateUrl(newUrl);

        // Assert
        link.Url.Should().Be(newUrl);
    }

    [Fact]
    public void UpdateUrl_ShouldNotChangeTargetOrPlatform()
    {
        // Arrange
        Guid lyricsId = Guid.NewGuid();
        StreamingLinkEntity link = StreamingLinkEntity.ForSingle(
            Guid.NewGuid(),
            lyricsId,
            EnumStreamingPlatform.YoutubeMusic,
            ValidUrl
        );

        // Act
        link.UpdateUrl("https://music.youtube.com/watch?v=xyz");

        // Assert
        link.LyricsId.Should().Be(lyricsId);
        link.Platform.Should().Be(EnumStreamingPlatform.YoutubeMusic);
    }

    #endregion
}
