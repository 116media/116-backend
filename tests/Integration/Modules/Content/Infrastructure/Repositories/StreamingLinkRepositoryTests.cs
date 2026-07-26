using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Infrastructure.Repositories;

/// <summary>
/// Integration tests for <see cref="IStreamingLinkRepository" /> verifying curated streaming
/// link persistence, lookup by album/single and platform, mutation and deletion against a real
/// PostgreSQL database.
/// </summary>
[Collection("Database")]
public class StreamingLinkRepositoryTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    /// <summary>
    /// Seeds a standalone album and returns its identifier.
    /// </summary>
    private async Task<Guid> SeedAlbumAsync()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var album = AlbumFactory.Create();
        context.Albums.Add(album);
        await context.SaveChangesAsync();

        return album.Id;
    }

    /// <summary>
    /// Seeds the content type, category and lyrics page required by a standalone single and
    /// returns the lyrics identifier.
    /// </summary>
    private async Task<Guid> SeedLyricsAsync()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        context.ContentTypes.Add(contentType);
        await context.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var lyrics = LyricsFactory.Create(category.Id);
        context.Lyrics.Add(lyrics);
        await context.SaveChangesAsync();

        return lyrics.Id;
    }

    [Fact]
    public async Task AddAsync_AlbumStreamingLink_PersistsToDatabase()
    {
        Guid albumId = await SeedAlbumAsync();
        var link = StreamingLinkFactory.CreateForAlbum(albumId, EnumStreamingPlatform.Spotify);

        var (repo, db) = CreateScopedRepository<IStreamingLinkRepository, ContentDbContext>();
        await repo.AddAsync(link);
        await db.SaveChangesAsync();

        await using var verifyContext = CreateDbContext<ContentDbContext>();
        StreamingLinkEntity? persisted = await verifyContext.StreamingLinks.FindAsync(link.Id);

        persisted.Should().NotBeNull();
        persisted!.AlbumId.Should().Be(albumId);
        persisted.LyricsId.Should().BeNull();
        persisted.Platform.Should().Be(EnumStreamingPlatform.Spotify);
    }

    [Fact]
    public async Task AddAsync_SingleStreamingLink_PersistsToDatabase()
    {
        Guid lyricsId = await SeedLyricsAsync();
        var link = StreamingLinkFactory.CreateForLyrics(lyricsId, EnumStreamingPlatform.Tidal);

        var (repo, db) = CreateScopedRepository<IStreamingLinkRepository, ContentDbContext>();
        await repo.AddAsync(link);
        await db.SaveChangesAsync();

        await using var verifyContext = CreateDbContext<ContentDbContext>();
        StreamingLinkEntity? persisted = await verifyContext.StreamingLinks.FindAsync(link.Id);

        persisted.Should().NotBeNull();
        persisted!.LyricsId.Should().Be(lyricsId);
        persisted.AlbumId.Should().BeNull();
        persisted.Platform.Should().Be(EnumStreamingPlatform.Tidal);
    }

    [Fact]
    public async Task GetByAlbumAndPlatformAsync_ExistingLink_ReturnsLink()
    {
        Guid albumId = await SeedAlbumAsync();
        var link = StreamingLinkFactory.CreateForAlbum(albumId, EnumStreamingPlatform.AppleMusic);

        await using var context = CreateDbContext<ContentDbContext>();
        context.StreamingLinks.Add(link);
        await context.SaveChangesAsync();

        var repo = Resolve<IStreamingLinkRepository>();

        StreamingLinkEntity? result = await repo.GetByAlbumAndPlatformAsync(albumId, EnumStreamingPlatform.AppleMusic);

        result.Should().NotBeNull();
        result!.Id.Should().Be(link.Id);
        result.Url.Should().Be(link.Url);
    }

    [Fact]
    public async Task GetByAlbumAndPlatformAsync_OtherPlatform_ReturnsNull()
    {
        Guid albumId = await SeedAlbumAsync();

        await using var context = CreateDbContext<ContentDbContext>();
        context.StreamingLinks.Add(StreamingLinkFactory.CreateForAlbum(albumId, EnumStreamingPlatform.Spotify));
        await context.SaveChangesAsync();

        var repo = Resolve<IStreamingLinkRepository>();

        StreamingLinkEntity? result = await repo.GetByAlbumAndPlatformAsync(albumId, EnumStreamingPlatform.Tidal);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByAlbumAsync_WithCuratedLinks_ReturnsUrlsKeyedByPlatform()
    {
        Guid albumId = await SeedAlbumAsync();

        await using var context = CreateDbContext<ContentDbContext>();
        context.StreamingLinks.AddRange(
            StreamingLinkFactory.CreateForAlbum(albumId, EnumStreamingPlatform.Spotify, "https://spotify.test/album"),
            StreamingLinkFactory.CreateForAlbum(albumId, EnumStreamingPlatform.Tidal, "https://tidal.test/album")
        );
        await context.SaveChangesAsync();

        var repo = Resolve<IStreamingLinkRepository>();

        IReadOnlyDictionary<EnumStreamingPlatform, string> result = await repo.GetByAlbumAsync(albumId);

        result.Should().HaveCount(2);
        result[EnumStreamingPlatform.Spotify].Should().Be("https://spotify.test/album");
        result[EnumStreamingPlatform.Tidal].Should().Be("https://tidal.test/album");
    }

    [Fact]
    public async Task GetByAlbumAsync_WithoutCuratedLinks_ReturnsEmptyDictionary()
    {
        Guid albumId = await SeedAlbumAsync();

        var repo = Resolve<IStreamingLinkRepository>();

        IReadOnlyDictionary<EnumStreamingPlatform, string> result = await repo.GetByAlbumAsync(albumId);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByLyricsAndPlatformAsync_ExistingLink_ReturnsLink()
    {
        Guid lyricsId = await SeedLyricsAsync();
        var link = StreamingLinkFactory.CreateForLyrics(lyricsId, EnumStreamingPlatform.YoutubeMusic);

        await using var context = CreateDbContext<ContentDbContext>();
        context.StreamingLinks.Add(link);
        await context.SaveChangesAsync();

        var repo = Resolve<IStreamingLinkRepository>();

        StreamingLinkEntity? result = await repo.GetByLyricsAndPlatformAsync(
            lyricsId,
            EnumStreamingPlatform.YoutubeMusic
        );

        result.Should().NotBeNull();
        result!.Id.Should().Be(link.Id);
        result.LyricsId.Should().Be(lyricsId);
    }

    [Fact]
    public async Task GetByLyricsAndPlatformAsync_OtherPlatform_ReturnsNull()
    {
        Guid lyricsId = await SeedLyricsAsync();

        await using var context = CreateDbContext<ContentDbContext>();
        context.StreamingLinks.Add(StreamingLinkFactory.CreateForLyrics(lyricsId, EnumStreamingPlatform.Spotify));
        await context.SaveChangesAsync();

        var repo = Resolve<IStreamingLinkRepository>();

        StreamingLinkEntity? result = await repo.GetByLyricsAndPlatformAsync(lyricsId, EnumStreamingPlatform.Tidal);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Update_ChangedUrl_PersistsNewUrl()
    {
        Guid albumId = await SeedAlbumAsync();
        var link = StreamingLinkFactory.CreateForAlbum(albumId, EnumStreamingPlatform.Spotify);

        await using (var seedContext = CreateDbContext<ContentDbContext>())
        {
            seedContext.StreamingLinks.Add(link);
            await seedContext.SaveChangesAsync();
        }

        var (repo, db) = CreateScopedRepository<IStreamingLinkRepository, ContentDbContext>();
        StreamingLinkEntity? tracked = await db.StreamingLinks.FirstAsync(x => x.Id == link.Id);
        tracked.UpdateUrl("https://open.spotify.com/album/updated-url");
        repo.Update(tracked);
        await db.SaveChangesAsync();

        await using var verifyContext = CreateDbContext<ContentDbContext>();
        StreamingLinkEntity? persisted = await verifyContext.StreamingLinks.FindAsync(link.Id);

        persisted.Should().NotBeNull();
        persisted!.Url.Should().Be("https://open.spotify.com/album/updated-url");
    }

    [Fact]
    public async Task Remove_ExistingLink_DeletesFromDatabase()
    {
        Guid lyricsId = await SeedLyricsAsync();
        var link = StreamingLinkFactory.CreateForLyrics(lyricsId, EnumStreamingPlatform.AppleMusic);

        await using (var seedContext = CreateDbContext<ContentDbContext>())
        {
            seedContext.StreamingLinks.Add(link);
            await seedContext.SaveChangesAsync();
        }

        var (repo, db) = CreateScopedRepository<IStreamingLinkRepository, ContentDbContext>();
        StreamingLinkEntity? tracked = await db.StreamingLinks.FirstAsync(x => x.Id == link.Id);
        repo.Remove(tracked);
        await db.SaveChangesAsync();

        await using var verifyContext = CreateDbContext<ContentDbContext>();
        bool exists = await verifyContext.StreamingLinks.AnyAsync(x => x.Id == link.Id);

        exists.Should().BeFalse();
    }
}
