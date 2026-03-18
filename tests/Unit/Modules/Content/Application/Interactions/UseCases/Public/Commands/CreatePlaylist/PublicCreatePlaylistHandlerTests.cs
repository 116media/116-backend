using _116.Content.Application.Interactions.Persistence;
using _116.Content.Application.Interactions.UseCases.Public.Commands.CreatePlaylist;
using _116.Content.Application.Shared.Persistence;
using _116.Tests.Fixtures.Constants;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.CreatePlaylist;

/// <summary>
/// Unit tests for <see cref="PublicCreatePlaylistHandler"/>.
/// </summary>
public class PublicCreatePlaylistHandlerTests
{
    private readonly Mock<IPlaylistRepository> _playlistRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly PublicCreatePlaylistHandler _handler;

    public PublicCreatePlaylistHandlerTests()
    {
        _playlistRepositoryMock = MockPlaylistRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new PublicCreatePlaylistHandler(_playlistRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenValid_ShouldCreatePlaylistAndCommit()
    {
        // Arrange
        var command = new PublicCreatePlaylistCommand(
            UserId: Guid.NewGuid(),
            Name: TestConstants.Content.Interactions.ValidPlaylistName
        );

        // Act
        PublicCreatePlaylistResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Playlist.Should().NotBeNull();
        result.Playlist.Name.Should().Be(command.Name);
        result.Playlist.VideoCount.Should().Be(0);
        _playlistRepositoryMock.VerifyAddCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion
}
