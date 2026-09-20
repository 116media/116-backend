using _116.Content.Application.Interactions.Persistence;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Infrastructure.Repositories;

/// <summary>
/// Integration tests for <see cref="IPlaylistRepository" /> verifying playlist CRUD,
/// video management, and user lookup against a real PostgreSQL database.
/// </summary>
[Collection("Database")]
public class PlaylistRepositoryTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    [Fact]
    public async Task AddAsync_NewPlaylist_PersistsToDatabase()
    {
        var userId = Guid.NewGuid();
        var playlist = PlaylistFactory.Create(userId);
        var (repo, db) = CreateScopedRepository<IPlaylistRepository, ContentDbContext>();

        await repo.AddAsync(playlist);
        await db.SaveChangesAsync();

        await using var verifyContext = CreateDbContext<ContentDbContext>();
        var persisted = await verifyContext.Playlists.FindAsync(playlist.Id);
        persisted.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ExistingPlaylist_ReturnsPlaylist()
    {
        var userId = Guid.NewGuid();
        await using var context = CreateDbContext<ContentDbContext>();
        var playlist = PlaylistFactory.Create(userId);
        context.Playlists.Add(playlist);
        await context.SaveChangesAsync();

        var repo = Resolve<IPlaylistRepository>();

        var result = await repo.GetByIdAsync(playlist.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(playlist.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentPlaylist_ReturnsNull()
    {
        var repo = Resolve<IPlaylistRepository>();

        var result = await repo.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByUserIdAsync_WithPlaylists_ReturnsUserPlaylists()
    {
        var userId = Guid.NewGuid();
        await using var context = CreateDbContext<ContentDbContext>();
        context.Playlists.Add(PlaylistFactory.Create(userId));
        context.Playlists.Add(PlaylistFactory.Create(userId));
        context.Playlists.Add(PlaylistFactory.Create(Guid.NewGuid()));
        await context.SaveChangesAsync();

        var repo = Resolve<IPlaylistRepository>();

        var result = await repo.GetByUserIdAsync(userId);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByUserIdAsync_NoPlaylists_ReturnsEmpty()
    {
        var repo = Resolve<IPlaylistRepository>();

        var result = await repo.GetByUserIdAsync(Guid.NewGuid());

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdWithVideosAsync_ExistingPlaylist_ReturnsPlaylistWithVideos()
    {
        var userId = Guid.NewGuid();
        await using var context = CreateDbContext<ContentDbContext>();
        var playlist = PlaylistFactory.Create(userId);
        context.Playlists.Add(playlist);
        await context.SaveChangesAsync();

        var repo = Resolve<IPlaylistRepository>();

        var result = await repo.GetByIdWithVideosAsync(playlist.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(playlist.Id);
    }

    [Fact]
    public async Task GetByIdWithVideosAsync_NonExistentPlaylist_ReturnsNull()
    {
        var repo = Resolve<IPlaylistRepository>();

        var result = await repo.GetByIdWithVideosAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task ContainsVideo_NoVideo_ReturnsFalse()
    {
        var playlist = PlaylistFactory.Create(Guid.NewGuid());
        await using var seedContext = CreateDbContext<ContentDbContext>();
        seedContext.Playlists.Add(playlist);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IPlaylistRepository>();
        PlaylistEntity loaded = (await repo.GetByIdAsync(playlist.Id))!;

        loaded.ContainsVideo(Guid.NewGuid()).Should().BeFalse();
    }

    [Fact]
    public async Task Delete_ExistingPlaylist_RemovesFromDatabase()
    {
        var userId = Guid.NewGuid();
        await using var context = CreateDbContext<ContentDbContext>();
        var playlist = PlaylistFactory.Create(userId);
        context.Playlists.Add(playlist);
        await context.SaveChangesAsync();

        var (repo, db) = CreateScopedRepository<IPlaylistRepository, ContentDbContext>();
        var toDelete = await db.Playlists.FindAsync(playlist.Id);
        repo.Delete(toDelete!);
        await db.SaveChangesAsync();

        await using var verifyContext = CreateDbContext<ContentDbContext>();
        var removed = await verifyContext.Playlists.FindAsync(playlist.Id);
        removed.Should().BeNull();
    }

    [Fact]
    public async Task ContainsVideo_AfterAddingThroughTheRoot_ReturnsTrue()
    {
        var userId = Guid.NewGuid();
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        var video = VideoFactory.Create(category.Id);
        seedContext.Videos.Add(video);
        await seedContext.SaveChangesAsync();

        var playlist = PlaylistFactory.Create(userId);
        playlist.AddVideo(videoId: video.Id, sortOrder: 1);
        seedContext.Playlists.Add(playlist);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IPlaylistRepository>();
        PlaylistEntity loaded = (await repo.GetByIdAsync(playlist.Id))!;

        loaded.ContainsVideo(video.Id).Should().BeTrue();
    }

    [Fact]
    public async Task AddVideo_ThroughTheRoot_PersistsToPlaylist()
    {
        var userId = Guid.NewGuid();
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        var video = VideoFactory.Create(category.Id);
        seedContext.Videos.Add(video);
        await seedContext.SaveChangesAsync();

        var playlist = PlaylistFactory.Create(userId);
        seedContext.Playlists.Add(playlist);
        await seedContext.SaveChangesAsync();

        var (repo, db) = CreateScopedRepository<IPlaylistRepository, ContentDbContext>();
        PlaylistEntity tracked = (await repo.GetByIdAsync(playlist.Id))!;

        tracked.AddVideo(videoId: video.Id, sortOrder: 1).Should().BeTrue();
        await db.SaveChangesAsync();

        await using var verifyContext = CreateDbContext<ContentDbContext>();
        var exists = await verifyContext.PlaylistVideos.AnyAsync(pv =>
            pv.PlaylistId == playlist.Id && pv.VideoId == video.Id
        );
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task Update_ExistingPlaylist_PersistsNameChange()
    {
        var userId = Guid.NewGuid();
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var playlist = PlaylistFactory.Create(userId);
        seedContext.Playlists.Add(playlist);
        await seedContext.SaveChangesAsync();

        var (repo, db) = CreateScopedRepository<IPlaylistRepository, ContentDbContext>();
        var toUpdate = await db.Playlists.FindAsync(playlist.Id);
        toUpdate!.Rename("Renamed Playlist");
        repo.Update(toUpdate);
        await db.SaveChangesAsync();

        await using var verifyContext = CreateDbContext<ContentDbContext>();
        var updated = await verifyContext.Playlists.FindAsync(playlist.Id);
        updated!.Name.Should().Be("Renamed Playlist");
    }
}
