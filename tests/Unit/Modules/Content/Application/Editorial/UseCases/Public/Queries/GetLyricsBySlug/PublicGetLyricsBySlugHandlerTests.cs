using _116.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsBySlug;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Core.Application.Shared.Repositories;
using _116.Identity.Contracts.Application;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using _116.Unit.Tests.Common.Mocks.Services;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsBySlug;

/// <summary>
/// Unit tests for <see cref="PublicGetLyricsBySlugHandler"/>.
/// </summary>
public class PublicGetLyricsBySlugHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ILyricsRepository> _lyricsRepositoryMock;
    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly Mock<IArtistRepository> _artistRepositoryMock;
    private readonly Mock<IAlbumRepository> _albumRepositoryMock;
    private readonly Mock<IStreamingLinkRepository> _streamingLinkRepositoryMock;
    private readonly PublicGetLyricsBySlugHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public PublicGetLyricsBySlugHandlerTests()
    {
        _lyricsRepositoryMock = MockLyricsRepository.Create();
        _videoRepositoryMock = MockVideoRepository.Create();
        _artistRepositoryMock = MockArtistRepository.Create();
        _albumRepositoryMock = MockAlbumRepository.Create();
        _streamingLinkRepositoryMock = MockStreamingLinkRepository.Create();
        Mock<IUserLookupService> userLookupMock = MockUserLookupService.Create();
        Mock<IFileRepository> fileRepositoryMock = MockFileRepository.Create();
        _handler = new PublicGetLyricsBySlugHandler(
            _lyricsRepositoryMock.Object,
            _videoRepositoryMock.Object,
            _artistRepositoryMock.Object,
            _albumRepositoryMock.Object,
            _streamingLinkRepositoryMock.Object,
            Mapper,
            userLookupMock.Object,
            fileRepositoryMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    [Fact]
    public async Task Handle_WhenLyricsFoundBySlug_ShouldReturnLyrics()
    {
        // Arrange
        string slug = TestConstants.Lyrics.ValidSlug;
        LyricsEntity lyrics = LyricsFactory.CreateWithSlug(CategoryId, slug);
        lyrics.Publish();
        var query = new PublicGetLyricsBySlugQuery(Slug: slug);

        _lyricsRepositoryMock.SetupGetBySlug(slug, lyrics);

        // Act
        PublicGetLyricsBySlugResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Lyrics.Should().NotBeNull();
        result.Lyrics.Slug.Should().Be(slug);
    }

    [Fact]
    public async Task Handle_WhenLyricsLinkedToExistingVideo_ShouldResolveVideoSlug()
    {
        // Arrange
        string slug = TestConstants.Lyrics.ValidSlug;
        Guid videoId = Guid.NewGuid();
        LyricsEntity lyrics = LyricsFactory.CreateForVideo(CategoryId, videoId);
        lyrics.Publish();
        VideoEntity video = VideoFactory.CreateWithSlug(CategoryId, "linked-video-slug");
        var query = new PublicGetLyricsBySlugQuery(Slug: slug);

        _lyricsRepositoryMock.SetupGetBySlug(slug, lyrics);
        _videoRepositoryMock.SetupGetByIdAsync(videoId, video);

        // Act
        PublicGetLyricsBySlugResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.VideoSlug.Should().Be("linked-video-slug");
    }

    [Fact]
    public async Task Handle_WhenLinkedVideoNoLongerExists_ShouldResolveNullVideoSlugWithoutThrowing()
    {
        // Arrange
        string slug = TestConstants.Lyrics.ValidSlug;
        Guid videoId = Guid.NewGuid();
        LyricsEntity lyrics = LyricsFactory.CreateForVideo(CategoryId, videoId);
        lyrics.Publish();
        var query = new PublicGetLyricsBySlugQuery(Slug: slug);

        _lyricsRepositoryMock.SetupGetBySlug(slug, lyrics);
        _videoRepositoryMock.SetupGetByIdAsync(videoId, null);

        // Act
        PublicGetLyricsBySlugResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.VideoSlug.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenLyricsStandalone_ShouldResolveNullVideoSlug()
    {
        // Arrange
        string slug = TestConstants.Lyrics.ValidSlug;
        LyricsEntity lyrics = LyricsFactory.CreateWithSlug(CategoryId, slug);
        lyrics.Publish();
        var query = new PublicGetLyricsBySlugQuery(Slug: slug);

        _lyricsRepositoryMock.SetupGetBySlug(slug, lyrics);

        // Act
        PublicGetLyricsBySlugResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.VideoSlug.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenLyricsLinkedToExistingArtist_ShouldResolveArtistSlug()
    {
        // Arrange
        string slug = TestConstants.Lyrics.ValidSlug;
        LyricsEntity lyrics = LyricsFactory.CreateWithSlug(CategoryId, slug);
        lyrics.Publish();
        ArtistEntity artist = ArtistFactory.CreateWithSlug("linked-artist-slug");
        lyrics.LinkArtist(artist.Id);
        var query = new PublicGetLyricsBySlugQuery(Slug: slug);

        _lyricsRepositoryMock.SetupGetBySlug(slug, lyrics);
        _artistRepositoryMock.SetupGetByIdAsync(artist.Id, artist);

        // Act
        PublicGetLyricsBySlugResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ArtistSlug.Should().Be("linked-artist-slug");
    }

    [Fact]
    public async Task Handle_WhenLinkedArtistNoLongerExists_ShouldResolveNullArtistSlugWithoutThrowing()
    {
        // Arrange
        string slug = TestConstants.Lyrics.ValidSlug;
        LyricsEntity lyrics = LyricsFactory.CreateWithSlug(CategoryId, slug);
        lyrics.Publish();
        Guid artistId = Guid.NewGuid();
        lyrics.LinkArtist(artistId);
        var query = new PublicGetLyricsBySlugQuery(Slug: slug);

        _lyricsRepositoryMock.SetupGetBySlug(slug, lyrics);
        _artistRepositoryMock.SetupGetByIdAsync(artistId, null);

        // Act
        PublicGetLyricsBySlugResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ArtistSlug.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenLyricsHasNoLinkedArtist_ShouldResolveNullArtistSlug()
    {
        // Arrange
        string slug = TestConstants.Lyrics.ValidSlug;
        LyricsEntity lyrics = LyricsFactory.CreateWithSlug(CategoryId, slug);
        lyrics.Publish();
        var query = new PublicGetLyricsBySlugQuery(Slug: slug);

        _lyricsRepositoryMock.SetupGetBySlug(slug, lyrics);

        // Act
        PublicGetLyricsBySlugResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ArtistSlug.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenLyricsFoundBySlugButNotPublished_ShouldThrowNotFoundException()
    {
        // Arrange
        string slug = TestConstants.Lyrics.ValidSlug;
        LyricsEntity draftLyrics = LyricsFactory.CreateWithSlug(CategoryId, slug);
        var query = new PublicGetLyricsBySlugQuery(Slug: slug);

        _lyricsRepositoryMock.SetupGetBySlug(slug, draftLyrics);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenNoLyricsMatchSlug_ShouldThrowNotFoundException()
    {
        // Arrange
        string slug = TestConstants.Lyrics.AnotherValidSlug;
        var query = new PublicGetLyricsBySlugQuery(Slug: slug);

        _lyricsRepositoryMock.SetupGetBySlug(slug, null);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenLyricsLinkedToAlbum_ShouldReturnSiblingAlbumTracksExcludingSelf()
    {
        // Arrange
        string slug = TestConstants.Lyrics.ValidSlug;
        Guid albumId = Guid.NewGuid();
        LyricsEntity lyrics = LyricsFactory.CreateWithSlug(CategoryId, slug);
        lyrics.LinkAlbum(albumId);
        lyrics.Publish();
        var query = new PublicGetLyricsBySlugQuery(Slug: slug);

        List<LyricsEntity> siblingTracks =
        [
            LyricsFactory.CreateWithSlug(CategoryId, "sibling-track-one"),
            LyricsFactory.CreateWithSlug(CategoryId, "sibling-track-two"),
        ];

        _lyricsRepositoryMock.SetupGetBySlug(slug, lyrics);
        _lyricsRepositoryMock.SetupGetPublishedByAlbumAsync(albumId, lyrics.Id, siblingTracks);

        // Act
        PublicGetLyricsBySlugResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.AlbumTracks.Should().HaveCount(2);
        result.AlbumTracks.Should().NotContain(t => t.Slug == slug);
        result.AlbumTracks.Select(t => t.Slug).Should().Contain(["sibling-track-one", "sibling-track-two"]);
    }

    [Fact]
    public async Task Handle_WhenLyricsHasNoAlbum_ShouldReturnEmptyAlbumTracks()
    {
        // Arrange
        string slug = TestConstants.Lyrics.ValidSlug;
        LyricsEntity lyrics = LyricsFactory.CreateWithSlug(CategoryId, slug);
        lyrics.Publish();
        var query = new PublicGetLyricsBySlugQuery(Slug: slug);

        _lyricsRepositoryMock.SetupGetBySlug(slug, lyrics);

        // Act
        PublicGetLyricsBySlugResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.AlbumTracks.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenLyricsLinkedToAlbum_ShouldReturnFiveStreamingLinks()
    {
        // Arrange
        string slug = TestConstants.Lyrics.ValidSlug;
        Guid albumId = Guid.NewGuid();
        LyricsEntity lyrics = LyricsFactory.CreateWithSlug(CategoryId, slug);
        lyrics.LinkAlbum(albumId);
        lyrics.Publish();
        var query = new PublicGetLyricsBySlugQuery(Slug: slug);

        _lyricsRepositoryMock.SetupGetBySlug(slug, lyrics);

        // Act
        PublicGetLyricsBySlugResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.StreamingLinks.Should().HaveCount(5);
    }

    [Fact]
    public async Task Handle_WhenLyricsStandalone_ShouldReturnFiveStreamingLinks()
    {
        // Arrange
        string slug = TestConstants.Lyrics.ValidSlug;
        LyricsEntity lyrics = LyricsFactory.CreateWithSlug(CategoryId, slug);
        lyrics.Publish();
        var query = new PublicGetLyricsBySlugQuery(Slug: slug);

        _lyricsRepositoryMock.SetupGetBySlug(slug, lyrics);

        // Act
        PublicGetLyricsBySlugResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.StreamingLinks.Should().HaveCount(5);
    }

    [Fact]
    public async Task Handle_WhenCuratedStreamingLinkExistsForStandaloneSingle_ShouldPreferCuratedOverGenerated()
    {
        // Arrange
        string slug = TestConstants.Lyrics.ValidSlug;
        LyricsEntity lyrics = LyricsFactory.CreateWithSlug(CategoryId, slug);
        lyrics.Publish();
        var query = new PublicGetLyricsBySlugQuery(Slug: slug);
        const string curatedUrl = "https://open.spotify.com/track/curated-abc123";

        _lyricsRepositoryMock.SetupGetBySlug(slug, lyrics);
        _streamingLinkRepositoryMock.SetupGetByLyricsAsync(
            lyrics.Id,
            new Dictionary<EnumStreamingPlatform, string> { [EnumStreamingPlatform.Spotify] = curatedUrl }
        );

        // Act
        PublicGetLyricsBySlugResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.StreamingLinks.Single(l => l.Platform == "Spotify").Url.Should().Be(curatedUrl);
    }

    [Fact]
    public async Task Handle_WhenCurrentUserHasLiked_ShouldReturnIsLikedTrue()
    {
        // Arrange
        string slug = TestConstants.Lyrics.ValidSlug;
        LyricsEntity lyrics = LyricsFactory.CreateWithSlug(CategoryId, slug);
        lyrics.Publish();
        Guid currentUserId = Guid.NewGuid();
        var query = new PublicGetLyricsBySlugQuery(Slug: slug, CurrentUserId: currentUserId);

        _lyricsRepositoryMock.SetupGetBySlug(slug, lyrics);
        _lyricsRepositoryMock.SetupHasLikedAsync(true);

        // Act
        PublicGetLyricsBySlugResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Lyrics.IsLiked.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenCurrentUserHasNotLiked_ShouldReturnIsLikedFalse()
    {
        // Arrange
        string slug = TestConstants.Lyrics.ValidSlug;
        LyricsEntity lyrics = LyricsFactory.CreateWithSlug(CategoryId, slug);
        lyrics.Publish();
        var query = new PublicGetLyricsBySlugQuery(Slug: slug, CurrentUserId: Guid.NewGuid());

        _lyricsRepositoryMock.SetupGetBySlug(slug, lyrics);
        _lyricsRepositoryMock.SetupHasLikedAsync(false);

        // Act
        PublicGetLyricsBySlugResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Lyrics.IsLiked.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenAnonymous_ShouldReturnIsLikedFalse()
    {
        // Arrange
        string slug = TestConstants.Lyrics.ValidSlug;
        LyricsEntity lyrics = LyricsFactory.CreateWithSlug(CategoryId, slug);
        lyrics.Publish();
        var query = new PublicGetLyricsBySlugQuery(Slug: slug, CurrentUserId: null);

        _lyricsRepositoryMock.SetupGetBySlug(slug, lyrics);
        _lyricsRepositoryMock.SetupHasLikedAsync(true);

        // Act
        PublicGetLyricsBySlugResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Lyrics.IsLiked.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldPassThroughViewLikeAndShareCounts()
    {
        // Arrange
        string slug = TestConstants.Lyrics.ValidSlug;
        LyricsEntity lyrics = LyricsFactory.CreateWithSlug(CategoryId, slug);
        lyrics.Publish();
        lyrics.IncrementViewCount();
        lyrics.IncrementViewCount();
        lyrics.IncrementLikeCount();
        lyrics.IncrementShareCount();
        lyrics.IncrementShareCount();
        lyrics.IncrementShareCount();
        var query = new PublicGetLyricsBySlugQuery(Slug: slug);

        _lyricsRepositoryMock.SetupGetBySlug(slug, lyrics);

        // Act
        PublicGetLyricsBySlugResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Lyrics.ViewCount.Should().Be(2);
        result.Lyrics.LikeCount.Should().Be(1);
        result.Lyrics.ShareCount.Should().Be(3);
    }
}
