using _116.Content.Application.Interactions.Persistence;
using _116.Content.Application.Interactions.UseCases.Public.Queries.GetPlaylistById;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Interactions.UseCases.Public.Queries.GetPlaylistById;

/// <summary>
/// Unit tests for <see cref="PublicGetPlaylistByIdHandler"/>.
/// </summary>
public class PublicGetPlaylistByIdHandlerTests : BaseContentHandlerTest
{
    private static readonly Guid UserId = Guid.NewGuid();

    private readonly Mock<IPlaylistRepository> _playlistRepositoryMock;
    private readonly PublicGetPlaylistByIdHandler _handler;

    public PublicGetPlaylistByIdHandlerTests()
    {
        _playlistRepositoryMock = MockPlaylistRepository.Create();
        _handler = new PublicGetPlaylistByIdHandler(_playlistRepositoryMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenPlaylistExistsAndOwner_ShouldReturnPlaylistDetail()
    {
        // Arrange
        Guid playlistId = Guid.NewGuid();
        PlaylistEntity playlist = PlaylistFactory.CreateWithId(playlistId, UserId);
        _playlistRepositoryMock.SetupGetByIdWithVideosAsync(playlist);

        var query = new PublicGetPlaylistByIdQuery(Id: playlistId, UserId: UserId);

        // Act
        PublicGetPlaylistByIdResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Playlist.Should().NotBeNull();
        result.Playlist.Id.Should().Be(playlist.Id);
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenPlaylistNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        _playlistRepositoryMock.SetupGetByIdWithVideosAsync(null);

        var query = new PublicGetPlaylistByIdQuery(Id: Guid.NewGuid(), UserId: UserId);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenNotOwner_ShouldThrowBadRequestException()
    {
        // Arrange
        Guid playlistId = Guid.NewGuid();
        PlaylistEntity playlist = PlaylistFactory.CreateWithId(playlistId, Guid.NewGuid());
        _playlistRepositoryMock.SetupGetByIdWithVideosAsync(playlist);

        var query = new PublicGetPlaylistByIdQuery(Id: playlistId, UserId: Guid.NewGuid());

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    #endregion
}
