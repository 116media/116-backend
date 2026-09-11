using _116.Content.Application.Interactions.Factories;
using _116.Content.Application.Interactions.Persistence;
using _116.Content.Application.Interactions.UseCases.Public.Queries.GetOwnPlaylists;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Contracts.Application.Services;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Factories.Core;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using _116.Unit.Tests.Common.Mocks.Services;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Interactions.UseCases.Public.Queries.GetOwnPlaylists;

/// <summary>
/// Unit tests for <see cref="PublicGetOwnPlaylistsHandler"/>.
/// </summary>
public class PublicGetOwnPlaylistsHandlerTests : BaseContentHandlerTest
{
    private static readonly Guid UserId = Guid.NewGuid();

    private readonly Mock<IPlaylistRepository> _playlistRepositoryMock;
    private readonly Mock<IFileStorageService> _fileStorageMock;
    private readonly PublicGetOwnPlaylistsHandler _handler;

    public PublicGetOwnPlaylistsHandlerTests()
    {
        _playlistRepositoryMock = MockPlaylistRepository.Create();
        _fileStorageMock = MockFileStorageService.Create();
        _handler = new PublicGetOwnPlaylistsHandler(
            _playlistRepositoryMock.Object,
            new PlaylistDtoFactory(Mapper, _fileStorageMock.Object)
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenUserHasPlaylists_ShouldReturnMappedPlaylists()
    {
        // Arrange
        IReadOnlyList<PlaylistEntity> playlists = PlaylistFactory.CreateMany(2, UserId);
        _playlistRepositoryMock.SetupGetByUserIdAsync(playlists);

        var query = new PublicGetOwnPlaylistsQuery(UserId: UserId);

        // Act
        PublicGetOwnPlaylistsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Playlists.Count.Should().Be(2);
    }

    [Fact]
    public async Task Handle_WhenUserHasNoPlaylists_ShouldReturnEmptyList()
    {
        // Arrange
        _playlistRepositoryMock.SetupGetByUserIdAsync(new List<PlaylistEntity>());

        var query = new PublicGetOwnPlaylistsQuery(UserId: UserId);

        // Act
        PublicGetOwnPlaylistsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Playlists.Should().BeEmpty();
    }

    #endregion
}
