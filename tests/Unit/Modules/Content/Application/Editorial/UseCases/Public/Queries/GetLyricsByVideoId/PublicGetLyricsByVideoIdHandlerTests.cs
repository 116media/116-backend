using _116.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsByVideoId;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Identity.Contracts.Application;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using _116.Unit.Tests.Common.Mocks.Services;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsByVideoId;

/// <summary>
/// Unit tests for <see cref="PublicGetLyricsByVideoIdHandler"/>.
/// </summary>
public class PublicGetLyricsByVideoIdHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ILyricsRepository> _lyricsRepositoryMock;
    private readonly PublicGetLyricsByVideoIdHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public PublicGetLyricsByVideoIdHandlerTests()
    {
        _lyricsRepositoryMock = MockLyricsRepository.Create();
        Mock<IUserLookupService> userLookupMock = MockUserLookupService.Create();
        Mock<IFileRepository> fileRepositoryMock = MockFileRepository.Create();
        _handler = new PublicGetLyricsByVideoIdHandler(
            _lyricsRepositoryMock.Object,
            Mapper,
            userLookupMock.Object,
            fileRepositoryMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    [Fact]
    public async Task Handle_WhenLyricsLinkedToVideo_ShouldReturnLyrics()
    {
        // Arrange
        Guid videoId = Guid.NewGuid();
        LyricsEntity lyrics = LyricsFactory.CreateForVideo(CategoryId, videoId);
        lyrics.Publish();
        var query = new PublicGetLyricsByVideoIdQuery(VideoId: videoId.ToString());

        _lyricsRepositoryMock.SetupGetByVideoIdAsync(videoId, lyrics);

        // Act
        PublicGetLyricsByVideoIdResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Lyrics.Should().NotBeNull();
        result.Lyrics.VideoId.Should().Be(videoId);
    }

    /// <summary>
    /// A lyrics page linked to a video but not yet Published must be invisible to this public
    /// endpoint, mirroring the by-slug lookup's status gate.
    /// </summary>
    [Fact]
    public async Task Handle_WhenLyricsLinkedToVideoButNotPublished_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid videoId = Guid.NewGuid();
        LyricsEntity draftLyrics = LyricsFactory.CreateForVideo(CategoryId, videoId);
        var query = new PublicGetLyricsByVideoIdQuery(VideoId: videoId.ToString());

        _lyricsRepositoryMock.SetupGetByVideoIdAsync(videoId, draftLyrics);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenNoLyricsLinkedToVideo_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid videoId = Guid.NewGuid();
        var query = new PublicGetLyricsByVideoIdQuery(VideoId: videoId.ToString());

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    /// <summary>
    /// The authenticated caller who liked the linked lyrics page must see <c>IsLiked: true</c>.
    /// </summary>
    [Fact]
    public async Task Handle_WhenCurrentUserHasLiked_ShouldReturnIsLikedTrue()
    {
        // Arrange
        Guid videoId = Guid.NewGuid();
        LyricsEntity lyrics = LyricsFactory.CreateForVideo(CategoryId, videoId);
        lyrics.Publish();
        var query = new PublicGetLyricsByVideoIdQuery(VideoId: videoId.ToString(), CurrentUserId: Guid.NewGuid());

        _lyricsRepositoryMock.SetupGetByVideoIdAsync(videoId, lyrics);
        _lyricsRepositoryMock.SetupHasLikedAsync(true);

        // Act
        PublicGetLyricsByVideoIdResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Lyrics.IsLiked.Should().BeTrue();
    }

    /// <summary>
    /// An anonymous caller must always see <c>IsLiked: false</c>, regardless of any like records.
    /// </summary>
    [Fact]
    public async Task Handle_WhenAnonymous_ShouldReturnIsLikedFalse()
    {
        // Arrange
        Guid videoId = Guid.NewGuid();
        LyricsEntity lyrics = LyricsFactory.CreateForVideo(CategoryId, videoId);
        lyrics.Publish();
        var query = new PublicGetLyricsByVideoIdQuery(VideoId: videoId.ToString(), CurrentUserId: null);

        _lyricsRepositoryMock.SetupGetByVideoIdAsync(videoId, lyrics);
        _lyricsRepositoryMock.SetupHasLikedAsync(true);

        // Act
        PublicGetLyricsByVideoIdResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Lyrics.IsLiked.Should().BeFalse();
    }

    /// <summary>
    /// The view/like/share interaction counters must pass through from the entity unchanged.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldPassThroughViewLikeAndShareCounts()
    {
        // Arrange
        Guid videoId = Guid.NewGuid();
        LyricsEntity lyrics = LyricsFactory.CreateForVideo(CategoryId, videoId);
        lyrics.Publish();
        lyrics.IncrementViewCount();
        lyrics.IncrementLikeCount();
        lyrics.IncrementLikeCount();
        var query = new PublicGetLyricsByVideoIdQuery(VideoId: videoId.ToString());

        _lyricsRepositoryMock.SetupGetByVideoIdAsync(videoId, lyrics);

        // Act
        PublicGetLyricsByVideoIdResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Lyrics.ViewCount.Should().Be(1);
        result.Lyrics.LikeCount.Should().Be(2);
        result.Lyrics.ShareCount.Should().Be(0);
    }
}
