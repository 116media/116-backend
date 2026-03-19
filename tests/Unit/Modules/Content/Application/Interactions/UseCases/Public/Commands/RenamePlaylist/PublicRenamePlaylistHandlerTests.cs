using _116.Content.Application.Interactions.Persistence;
using _116.Content.Application.Interactions.UseCases.Public.Commands.RenamePlaylist;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.RenamePlaylist;

/// <summary>
/// Unit tests for <see cref="PublicRenamePlaylistHandler"/>.
/// </summary>
public class PublicRenamePlaylistHandlerTests
{
    private readonly Mock<IPlaylistRepository> _playlistRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly PublicRenamePlaylistHandler _handler;

    public PublicRenamePlaylistHandlerTests()
    {
        _playlistRepositoryMock = MockPlaylistRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new PublicRenamePlaylistHandler(_playlistRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenPlaylistExistsAndOwner_ShouldRenameAndCommit()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid playlistId = Guid.NewGuid();
        PlaylistEntity playlist = PlaylistFactory.CreateWithId(playlistId, userId);
        _playlistRepositoryMock.SetupGetByIdAsync(playlist);

        var command = new PublicRenamePlaylistCommand(Id: playlistId, UserId: userId, Name: "New Name");

        // Act
        PublicRenamePlaylistResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _playlistRepositoryMock.VerifyUpdateCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenPlaylistNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        _playlistRepositoryMock.SetupGetByIdAsync(null);

        var command = new PublicRenamePlaylistCommand(Id: Guid.NewGuid(), UserId: Guid.NewGuid(), Name: "New Name");

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

        var command = new PublicRenamePlaylistCommand(Id: playlistId, UserId: Guid.NewGuid(), Name: "New Name");

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    #endregion
}
