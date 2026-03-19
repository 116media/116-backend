using _116.Content.Application.Interactions.Persistence;
using _116.Content.Application.Interactions.UseCases.Public.Queries.GetMyPlaylists;
using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Interactions.UseCases.Public.Queries.GetMyPlaylists;

/// <summary>
/// Unit tests for <see cref="PublicGetMyPlaylistsHandler"/>.
/// </summary>
public class PublicGetMyPlaylistsHandlerTests : BaseContentHandlerTest
{
    private static readonly Guid UserId = Guid.NewGuid();

    private readonly Mock<IPlaylistRepository> _playlistRepositoryMock;
    private readonly PublicGetMyPlaylistsHandler _handler;

    public PublicGetMyPlaylistsHandlerTests()
    {
        _playlistRepositoryMock = MockPlaylistRepository.Create();
        _handler = new PublicGetMyPlaylistsHandler(_playlistRepositoryMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenUserHasPlaylists_ShouldReturnMappedPlaylists()
    {
        // Arrange
        IReadOnlyList<PlaylistEntity> playlists = PlaylistFactory.CreateMany(2, UserId);
        _playlistRepositoryMock.SetupGetByUserIdAsync(playlists);

        var query = new PublicGetMyPlaylistsQuery(UserId: UserId);

        // Act
        PublicGetMyPlaylistsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Playlists.Count.Should().Be(2);
    }

    [Fact]
    public async Task Handle_WhenUserHasNoPlaylists_ShouldReturnEmptyList()
    {
        // Arrange
        _playlistRepositoryMock.SetupGetByUserIdAsync(new List<PlaylistEntity>());

        var query = new PublicGetMyPlaylistsQuery(UserId: UserId);

        // Act
        PublicGetMyPlaylistsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Playlists.Should().BeEmpty();
    }

    #endregion
}
