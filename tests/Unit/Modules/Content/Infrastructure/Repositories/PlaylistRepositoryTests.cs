using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Content.Infrastructure.Repositories;
using _116.Tests.Fixtures.Factories.Content;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Infrastructure.Repositories;

/// <summary>
/// Unit tests for <see cref="PlaylistRepository"/> using InMemory database.
/// </summary>
public class PlaylistRepositoryTests : IDisposable
{
    private readonly ContentDbContext _context;
    private readonly PlaylistRepository _repository;

    public PlaylistRepositoryTests()
    {
        DbContextOptions<ContentDbContext> options = new DbContextOptionsBuilder<ContentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ContentDbContext(options);
        _repository = new PlaylistRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task<PlaylistEntity> SeedPlaylistAsync(Guid userId)
    {
        PlaylistEntity playlist = PlaylistFactory.Create(userId);
        _context.Playlists.Add(playlist);
        await _context.SaveChangesAsync();
        return playlist;
    }

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_ShouldPersistPlaylistEntity()
    {
        // Arrange
        var userId = Guid.NewGuid();
        PlaylistEntity playlist = PlaylistFactory.Create(userId);

        // Act
        await _repository.AddAsync(playlist);
        await _context.SaveChangesAsync();

        // Assert
        PlaylistEntity? retrieved = await _context.Playlists.FindAsync(playlist.Id);
        retrieved.Should().NotBeNull();
    }

    #endregion

    #region GetByUserIdAsync Tests

    [Fact]
    public async Task GetByUserIdAsync_WhenUserHasPlaylists_ShouldReturnPlaylists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _context.Playlists.Add(PlaylistFactory.Create(userId));
        _context.Playlists.Add(PlaylistFactory.Create(userId));
        await _context.SaveChangesAsync();

        // Act
        IReadOnlyList<PlaylistEntity> result = await _repository.GetByUserIdAsync(userId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(p => p.UserId.Should().Be(userId));
    }

    [Fact]
    public async Task GetByUserIdAsync_WhenUserHasNoPlaylists_ShouldReturnEmptyList()
    {
        // Act
        IReadOnlyList<PlaylistEntity> result = await _repository.GetByUserIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenFound_ShouldReturnEntity()
    {
        // Arrange
        PlaylistEntity playlist = await SeedPlaylistAsync(Guid.NewGuid());

        // Act
        PlaylistEntity? result = await _repository.GetByIdAsync(playlist.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(playlist.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ShouldReturnNull()
    {
        // Act
        PlaylistEntity? result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetByIdWithVideosAsync Tests

    [Fact]
    public async Task GetByIdWithVideosAsync_WhenFound_ShouldReturnPlaylist()
    {
        // Arrange
        PlaylistEntity playlist = await SeedPlaylistAsync(Guid.NewGuid());

        // Act
        PlaylistEntity? result = await _repository.GetByIdWithVideosAsync(playlist.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(playlist.Id);
    }

    [Fact]
    public async Task GetByIdWithVideosAsync_WhenNotFound_ShouldReturnNull()
    {
        // Act
        PlaylistEntity? result = await _repository.GetByIdWithVideosAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region VideoExistsInPlaylistAsync Tests

    [Fact]
    public async Task VideoExistsInPlaylistAsync_WhenVideoExists_ShouldReturnTrue()
    {
        // Arrange
        PlaylistEntity playlist = await SeedPlaylistAsync(Guid.NewGuid());
        var videoId = Guid.NewGuid();
        PlaylistVideoEntity playlistVideo = PlaylistVideoEntity.Create(
            Guid.NewGuid(),
            playlist.Id,
            videoId,
            sortOrder: 0
        );
        _context.PlaylistVideos.Add(playlistVideo);
        await _context.SaveChangesAsync();

        // Act
        bool result = await _repository.VideoExistsInPlaylistAsync(playlist.Id, videoId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task VideoExistsInPlaylistAsync_WhenVideoDoesNotExist_ShouldReturnFalse()
    {
        // Act
        bool result = await _repository.VideoExistsInPlaylistAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region AddVideoAsync Tests

    [Fact]
    public async Task AddVideoAsync_ShouldPersistPlaylistVideoEntity()
    {
        // Arrange
        PlaylistEntity playlist = await SeedPlaylistAsync(Guid.NewGuid());
        PlaylistVideoEntity playlistVideo = PlaylistVideoEntity.Create(
            Guid.NewGuid(),
            playlist.Id,
            Guid.NewGuid(),
            sortOrder: 0
        );

        // Act
        await _repository.AddVideoAsync(playlistVideo);
        await _context.SaveChangesAsync();

        // Assert
        PlaylistVideoEntity? retrieved = await _context.PlaylistVideos.FindAsync(playlistVideo.Id);
        retrieved.Should().NotBeNull();
    }

    #endregion

    #region RemoveVideoAsync Tests

    [Fact]
    public async Task RemoveVideoAsync_WhenEntryExists_ShouldRemoveFromDatabase()
    {
        // Arrange
        PlaylistEntity playlist = await SeedPlaylistAsync(Guid.NewGuid());
        var videoId = Guid.NewGuid();
        PlaylistVideoEntity playlistVideo = PlaylistVideoEntity.Create(
            Guid.NewGuid(),
            playlist.Id,
            videoId,
            sortOrder: 0
        );
        _context.PlaylistVideos.Add(playlistVideo);
        await _context.SaveChangesAsync();

        // Act
        await _repository.RemoveVideoAsync(playlist.Id, videoId);
        await _context.SaveChangesAsync();

        // Assert
        PlaylistVideoEntity? retrieved = await _context.PlaylistVideos.FindAsync(playlistVideo.Id);
        retrieved.Should().BeNull();
    }

    [Fact]
    public async Task RemoveVideoAsync_WhenEntryDoesNotExist_ShouldDoNothing()
    {
        // Act
        Func<Task> act = async () =>
        {
            await _repository.RemoveVideoAsync(Guid.NewGuid(), Guid.NewGuid());
            await _context.SaveChangesAsync();
        };

        // Assert
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_ShouldPersistChanges()
    {
        // Arrange
        PlaylistEntity playlist = await SeedPlaylistAsync(Guid.NewGuid());
        playlist.Rename("Updated Playlist Name");

        // Act
        _repository.Update(playlist);
        await _context.SaveChangesAsync();

        // Assert
        PlaylistEntity? retrieved = await _context.Playlists.FindAsync(playlist.Id);
        retrieved.Should().NotBeNull();
        retrieved.Name.Should().Be("Updated Playlist Name");
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_ShouldRemovePlaylistFromDatabase()
    {
        // Arrange
        PlaylistEntity playlist = await SeedPlaylistAsync(Guid.NewGuid());

        // Act
        _repository.Delete(playlist);
        await _context.SaveChangesAsync();

        // Assert
        PlaylistEntity? retrieved = await _context.Playlists.FindAsync(playlist.Id);
        retrieved.Should().BeNull();
    }

    #endregion
}
