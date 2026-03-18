using _116.Content.Application.Interactions.Persistence;
using _116.Content.Application.Interactions.UseCases.Public.Commands.AddVideoToPlaylist;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.AddVideoToPlaylist;

/// <summary>
/// Unit tests for <see cref="PublicAddVideoToPlaylistHandler"/>.
/// </summary>
public class PublicAddVideoToPlaylistHandlerTests
{
    private static readonly Guid CategoryId = Guid.NewGuid();

    private readonly Mock<IPlaylistRepository> _playlistRepositoryMock;
    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly PublicAddVideoToPlaylistHandler _handler;

    public PublicAddVideoToPlaylistHandlerTests()
    {
        _playlistRepositoryMock = MockPlaylistRepository.Create();
        _videoRepositoryMock = MockVideoRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new PublicAddVideoToPlaylistHandler(
            _playlistRepositoryMock.Object,
            _videoRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenAllValid_ShouldAddVideoAndCommit()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid playlistId = Guid.NewGuid();
        PlaylistEntity playlist = PlaylistFactory.CreateWithId(playlistId, userId);
        VideoEntity video = VideoFactory.CreatePublished(CategoryId);

        _playlistRepositoryMock.SetupGetByIdAsync(playlist);
        _videoRepositoryMock.SetupGetByIdOrThrow(video);
        _playlistRepositoryMock.SetupVideoExistsInPlaylistAsync(false);

        var command = new PublicAddVideoToPlaylistCommand(
            PlaylistId: playlistId,
            VideoId: video.Id,
            UserId: userId,
            SortOrder: 1
        );

        // Act
        PublicAddVideoToPlaylistResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _playlistRepositoryMock.VerifyAddVideoAsyncCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenPlaylistNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        _playlistRepositoryMock.SetupGetByIdAsync(null);

        var command = new PublicAddVideoToPlaylistCommand(
            PlaylistId: Guid.NewGuid(),
            VideoId: Guid.NewGuid(),
            UserId: Guid.NewGuid(),
            SortOrder: 1
        );

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenNotOwner_ShouldThrowBadRequestException()
    {
        // Arrange
        Guid playlistId = Guid.NewGuid();
        PlaylistEntity playlist = PlaylistFactory.CreateWithId(playlistId, Guid.NewGuid());
        _playlistRepositoryMock.SetupGetByIdAsync(playlist);

        var command = new PublicAddVideoToPlaylistCommand(
            PlaylistId: playlistId,
            VideoId: Guid.NewGuid(),
            UserId: Guid.NewGuid(),
            SortOrder: 1
        );

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenVideoNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid playlistId = Guid.NewGuid();
        Guid videoId = Guid.NewGuid();
        PlaylistEntity playlist = PlaylistFactory.CreateWithId(playlistId, userId);

        _playlistRepositoryMock.SetupGetByIdAsync(playlist);
        _videoRepositoryMock.SetupGetByIdOrThrowNotFound(videoId);

        var command = new PublicAddVideoToPlaylistCommand(
            PlaylistId: playlistId,
            VideoId: videoId,
            UserId: userId,
            SortOrder: 1
        );

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenVideoNotPublished_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid playlistId = Guid.NewGuid();
        PlaylistEntity playlist = PlaylistFactory.CreateWithId(playlistId, userId);
        VideoEntity draftVideo = VideoFactory.Create(CategoryId);

        _playlistRepositoryMock.SetupGetByIdAsync(playlist);
        _videoRepositoryMock.SetupGetByIdOrThrow(draftVideo);

        var command = new PublicAddVideoToPlaylistCommand(
            PlaylistId: playlistId,
            VideoId: draftVideo.Id,
            UserId: userId,
            SortOrder: 1
        );

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenVideoAlreadyInPlaylist_ShouldThrowConflictException()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid playlistId = Guid.NewGuid();
        PlaylistEntity playlist = PlaylistFactory.CreateWithId(playlistId, userId);
        VideoEntity video = VideoFactory.CreatePublished(CategoryId);

        _playlistRepositoryMock.SetupGetByIdAsync(playlist);
        _videoRepositoryMock.SetupGetByIdOrThrow(video);
        _playlistRepositoryMock.SetupVideoExistsInPlaylistAsync(true);

        var command = new PublicAddVideoToPlaylistCommand(
            PlaylistId: playlistId,
            VideoId: video.Id,
            UserId: userId,
            SortOrder: 1
        );

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    #endregion
}
