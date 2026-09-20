using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Content.Infrastructure.Repositories;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common.Helpers;
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
            .AddInterceptors(new CreatedAtStampingInterceptor())
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

    #region Video membership through the root

    [Fact]
    public async Task ContainsVideo_WhenVideoExists_ShouldReturnTrue()
    {
        // Arrange
        PlaylistEntity playlist = await SeedPlaylistAsync(Guid.NewGuid());
        var videoId = Guid.NewGuid();
        playlist.AddVideo(videoId: videoId, sortOrder: 0);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        PlaylistEntity loaded = (await _repository.GetByIdAsync(playlist.Id))!;

        // Assert
        loaded.ContainsVideo(videoId).Should().BeTrue();
        loaded.ContainsVideo(Guid.NewGuid()).Should().BeFalse();
    }

    [Fact]
    public async Task AddVideo_ThroughTheRoot_ShouldPersistTheMembershipRow()
    {
        // Arrange
        PlaylistEntity playlist = await SeedPlaylistAsync(Guid.NewGuid());
        var videoId = Guid.NewGuid();

        // Act
        playlist.AddVideo(videoId: videoId, sortOrder: 0).Should().BeTrue();
        await _context.SaveChangesAsync();

        // Assert
        (await _context.PlaylistVideos.AnyAsync(pv => pv.PlaylistId == playlist.Id && pv.VideoId == videoId))
            .Should()
            .BeTrue();
    }

    [Fact]
    public async Task AddVideo_WhenAlreadyInPlaylist_ShouldReportNoChange()
    {
        // Arrange
        PlaylistEntity playlist = await SeedPlaylistAsync(Guid.NewGuid());
        var videoId = Guid.NewGuid();
        playlist.AddVideo(videoId: videoId, sortOrder: 0);
        await _context.SaveChangesAsync();

        // Act & Assert
        playlist.AddVideo(videoId: videoId, sortOrder: 1).Should().BeFalse();
    }

    [Fact]
    public async Task RemoveVideo_ThroughTheRoot_ShouldDeleteTheMembershipRow()
    {
        // Arrange
        PlaylistEntity playlist = await SeedPlaylistAsync(Guid.NewGuid());
        var videoId = Guid.NewGuid();
        playlist.AddVideo(videoId: videoId, sortOrder: 0);
        await _context.SaveChangesAsync();

        // Act
        playlist.RemoveVideo(videoId).Should().BeTrue();
        await _context.SaveChangesAsync();

        // Assert
        (await _context.PlaylistVideos.AnyAsync(pv => pv.PlaylistId == playlist.Id))
            .Should()
            .BeFalse();
    }

    [Fact]
    public async Task RemoveVideo_WhenNotInPlaylist_ShouldReportNoChange()
    {
        // Arrange
        PlaylistEntity playlist = await SeedPlaylistAsync(Guid.NewGuid());

        // Act & Assert
        playlist.RemoveVideo(Guid.NewGuid()).Should().BeFalse();
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
