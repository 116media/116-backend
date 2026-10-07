using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsBySlug;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsBySlug.Contracts;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Mocks.Repositories;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Editorial.UseCases.Public.Queries.GetLyricsBySlug;

/// <summary>
/// Unit tests for <see cref="PublicLyricsPageService"/>: the linked slugs, the album siblings and
/// the streaming links of a lyrics page.
/// </summary>
public class PublicLyricsPageServiceTests
{
    private readonly Mock<IVideoRepository> _videoRepositoryMock = MockVideoRepository.Create();
    private readonly Mock<IArtistRepository> _artistRepositoryMock = MockArtistRepository.Create();
    private readonly Mock<IAlbumRepository> _albumRepositoryMock = MockAlbumRepository.Create();
    private readonly Mock<IStreamingLinkRepository> _streamingLinkRepositoryMock = MockStreamingLinkRepository.Create();
    private readonly Mock<ILyricsRepository> _lyricsRepositoryMock = MockLyricsRepository.Create();
    private readonly PublicLyricsPageService _service;

    public PublicLyricsPageServiceTests()
    {
        _service = new PublicLyricsPageService(
            _videoRepositoryMock.Object,
            _artistRepositoryMock.Object,
            _albumRepositoryMock.Object,
            _streamingLinkRepositoryMock.Object,
            _lyricsRepositoryMock.Object
        );
    }

    [Fact]
    public async Task ResolveLinksAsync_ForASingle_ShouldCarryNoAlbumTracksAndOneLinkPerPlatform()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(Guid.NewGuid());
        _streamingLinkRepositoryMock.SetupGetByLyricsAsync(lyrics.Id, new Dictionary<EnumStreamingPlatform, string>());

        // Act
        LyricsPageLinks links = await _service.ResolveLinksAsync(lyrics, CancellationToken.None);

        // Assert
        links.VideoSlug.Should().BeNull();
        links.ArtistSlug.Should().BeNull();
        links.AlbumTracks.Should().BeEmpty();
        links.StreamingLinks.Should().HaveCount(Enum.GetValues<EnumStreamingPlatform>().Length);
    }

    [Fact]
    public async Task ResolveLinksAsync_ForAnAlbumTrack_ShouldListTheSiblingTracks()
    {
        // Arrange
        Guid categoryId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        LyricsEntity lyrics = LyricsFactory.CreateForAlbum(categoryId, albumId);
        LyricsEntity sibling = LyricsFactory.CreateForAlbum(categoryId, albumId);
        _albumRepositoryMock.SetupGetByIdAsync(albumId, null);
        _lyricsRepositoryMock.SetupGetPublishedByAlbumAsync(albumId, lyrics.Id, [sibling]);
        _streamingLinkRepositoryMock.SetupGetByAlbumAsync(albumId, new Dictionary<EnumStreamingPlatform, string>());

        // Act
        LyricsPageLinks links = await _service.ResolveLinksAsync(lyrics, CancellationToken.None);

        // Assert
        links.AlbumTracks.Should().ContainSingle(track => track.SongTitle == sibling.SongTitle);
        links.StreamingLinks.Should().HaveCount(Enum.GetValues<EnumStreamingPlatform>().Length);
    }

    [Fact]
    public async Task ResolveLinksAsync_WithALinkedVideoAndArtist_ShouldCarryTheirSlugs()
    {
        // Arrange
        Guid categoryId = Guid.NewGuid();
        VideoEntity video = VideoFactory.CreateWithSlug(categoryId, "video-slug");
        ArtistEntity artist = ArtistFactory.Create("Name", "artist-slug");
        LyricsEntity lyrics = LyricsFactory.Create(categoryId);
        lyrics.Relink(videoId: video.Id);
        lyrics.LinkArtist(artistId: artist.Id);
        _videoRepositoryMock.SetupGetByIdAsync(video.Id, video);
        _artistRepositoryMock.SetupGetByIdAsync(artist.Id, artist);
        _streamingLinkRepositoryMock.SetupGetByLyricsAsync(lyrics.Id, new Dictionary<EnumStreamingPlatform, string>());

        // Act
        LyricsPageLinks links = await _service.ResolveLinksAsync(lyrics, CancellationToken.None);

        // Assert
        links.VideoSlug.Should().Be("video-slug");
        links.ArtistSlug.Should().Be("artist-slug");
    }
}
