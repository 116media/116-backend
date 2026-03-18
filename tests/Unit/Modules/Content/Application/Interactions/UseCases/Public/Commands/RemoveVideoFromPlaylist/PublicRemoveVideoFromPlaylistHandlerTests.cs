using _116.Content.Application.Interactions.Persistence;
using _116.Content.Application.Interactions.UseCases.Public.Commands.RemoveVideoFromPlaylist;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.RemoveVideoFromPlaylist;

/// <summary>
/// Unit tests for <see cref="PublicRemoveVideoFromPlaylistHandler"/>.
/// </summary>
public class PublicRemoveVideoFromPlaylistHandlerTests
{
    private readonly Mock<IPlaylistRepository> _playlistRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly PublicRemoveVideoFromPlaylistHandler _handler;

    public PublicRemoveVideoFromPlaylistHandlerTests()
    {
        _playlistRepositoryMock = MockPlaylistRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new PublicRemoveVideoFromPlaylistHandler(_playlistRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenPlaylistExistsAndOwner_ShouldRemoveVideoAndCommit()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid playlistId = Guid.NewGuid();
        Guid videoId = Guid.NewGuid();
        PlaylistEntity playlist = PlaylistFactory.CreateWithId(playlistId, userId);
        _playlistRepositoryMock.SetupGetByIdAsync(playlist);

        var command = new PublicRemoveVideoFromPlaylistCommand(
            PlaylistId: playlistId,
            VideoId: videoId,
            UserId: userId
        );

        // Act
        PublicRemoveVideoFromPlaylistResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _playlistRepositoryMock.VerifyRemoveVideoAsyncCalled(playlistId, videoId);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenPlaylistNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        _playlistRepositoryMock.SetupGetByIdAsync(null);

        var command = new PublicRemoveVideoFromPlaylistCommand(
            PlaylistId: Guid.NewGuid(),
            VideoId: Guid.NewGuid(),
            UserId: Guid.NewGuid()
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

        var command = new PublicRemoveVideoFromPlaylistCommand(
            PlaylistId: playlistId,
            VideoId: Guid.NewGuid(),
            UserId: Guid.NewGuid()
        );

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    #endregion
}
