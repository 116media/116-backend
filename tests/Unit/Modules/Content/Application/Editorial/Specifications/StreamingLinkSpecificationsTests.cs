using _116.Content.Application.Editorial.Specifications;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Tests.Fixtures.Factories.Content;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.Specifications;

/// <summary>
/// Unit tests for streaming link specification classes.
/// </summary>
public class StreamingLinkSpecificationsTests
{
    #region StreamingLinkByAlbumAndPlatformSpecification

    [Fact]
    public void StreamingLinkByAlbumAndPlatformSpecification_WithMatchingAlbumAndPlatform_ShouldReturnTrue()
    {
        // Arrange
        Guid albumId = Guid.NewGuid();
        StreamingLinkEntity link = StreamingLinkFactory.CreateForAlbum(albumId, EnumStreamingPlatform.Spotify);
        var spec = new StreamingLinkByAlbumAndPlatformSpecification(albumId, EnumStreamingPlatform.Spotify);

        // Act
        bool result = spec.IsSatisfiedBy(link);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void StreamingLinkByAlbumAndPlatformSpecification_WithDifferentPlatform_ShouldReturnFalse()
    {
        // Arrange
        Guid albumId = Guid.NewGuid();
        StreamingLinkEntity link = StreamingLinkFactory.CreateForAlbum(albumId, EnumStreamingPlatform.Spotify);
        var spec = new StreamingLinkByAlbumAndPlatformSpecification(albumId, EnumStreamingPlatform.Tidal);

        // Act
        bool result = spec.IsSatisfiedBy(link);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region StreamingLinkByAlbumSpecification

    [Fact]
    public void StreamingLinkByAlbumSpecification_WithMatchingAlbumId_ShouldReturnTrue()
    {
        // Arrange
        Guid albumId = Guid.NewGuid();
        StreamingLinkEntity link = StreamingLinkFactory.CreateForAlbum(albumId);
        var spec = new StreamingLinkByAlbumSpecification(albumId);

        // Act
        bool result = spec.IsSatisfiedBy(link);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void StreamingLinkByAlbumSpecification_WithDifferentAlbumId_ShouldReturnFalse()
    {
        // Arrange
        StreamingLinkEntity link = StreamingLinkFactory.CreateForAlbum(Guid.NewGuid());
        var spec = new StreamingLinkByAlbumSpecification(Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(link);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region StreamingLinkByLyricsAndPlatformSpecification

    [Fact]
    public void StreamingLinkByLyricsAndPlatformSpecification_WithMatchingLyricsAndPlatform_ShouldReturnTrue()
    {
        // Arrange
        Guid lyricsId = Guid.NewGuid();
        StreamingLinkEntity link = StreamingLinkFactory.CreateForLyrics(lyricsId, EnumStreamingPlatform.AppleMusic);
        var spec = new StreamingLinkByLyricsAndPlatformSpecification(lyricsId, EnumStreamingPlatform.AppleMusic);

        // Act
        bool result = spec.IsSatisfiedBy(link);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void StreamingLinkByLyricsAndPlatformSpecification_WithDifferentPlatform_ShouldReturnFalse()
    {
        // Arrange
        Guid lyricsId = Guid.NewGuid();
        StreamingLinkEntity link = StreamingLinkFactory.CreateForLyrics(lyricsId, EnumStreamingPlatform.AppleMusic);
        var spec = new StreamingLinkByLyricsAndPlatformSpecification(lyricsId, EnumStreamingPlatform.YoutubeMusic);

        // Act
        bool result = spec.IsSatisfiedBy(link);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region StreamingLinkByLyricsSpecification

    [Fact]
    public void StreamingLinkByLyricsSpecification_WithMatchingLyricsId_ShouldReturnTrue()
    {
        // Arrange
        Guid lyricsId = Guid.NewGuid();
        StreamingLinkEntity link = StreamingLinkFactory.CreateForLyrics(lyricsId);
        var spec = new StreamingLinkByLyricsSpecification(lyricsId);

        // Act
        bool result = spec.IsSatisfiedBy(link);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void StreamingLinkByLyricsSpecification_WithAlbumLink_ShouldReturnFalse()
    {
        // Arrange
        StreamingLinkEntity link = StreamingLinkFactory.CreateForAlbum(Guid.NewGuid());
        var spec = new StreamingLinkByLyricsSpecification(Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(link);

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}
