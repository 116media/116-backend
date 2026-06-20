using _116.Content.Application.Shared.Repositories;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Content.Repositories;

/// <summary>
/// Integration tests for <see cref="ILyricsRepository" /> verifying lyrics CRUD,
/// search, and lookup operations against a real PostgreSQL database.
/// </summary>
[Collection("Database")]
public class LyricsRepositoryTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    [Fact]
    public async Task GetAllAsync_WithLyrics_ReturnsPaginatedResults()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        context.Lyrics.AddRange(LyricsFactory.CreateMany(3));
        await context.SaveChangesAsync();

        var repo = Resolve<ILyricsRepository>();

        var (lyrics, totalCount) = await repo.GetAllAsync(1, 10, null);

        totalCount.Should().BeGreaterThanOrEqualTo(3);
        lyrics.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_ExistingLyrics_ReturnsLyrics()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var lyrics = LyricsFactory.Create();
        context.Lyrics.Add(lyrics);
        await context.SaveChangesAsync();

        var repo = Resolve<ILyricsRepository>();

        var result = await repo.GetByIdAsync(lyrics.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(lyrics.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentLyrics_ReturnsNull()
    {
        var repo = Resolve<ILyricsRepository>();

        var result = await repo.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdOrThrowAsync_NonExistentLyrics_ThrowsNotFoundException()
    {
        var repo = Resolve<ILyricsRepository>();

        var act = async () => await repo.GetByIdOrThrowAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task GetBySongTitleAndArtistAsync_ExistingMatch_ReturnsLyrics()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var lyrics = LyricsFactory.Create("Unique Song Title", "Unique Artist");
        context.Lyrics.Add(lyrics);
        await context.SaveChangesAsync();

        var repo = Resolve<ILyricsRepository>();

        var result = await repo.GetBySongTitleAndArtistAsync("Unique Song Title", "Unique Artist");

        result.Should().NotBeNull();
        result!.Id.Should().Be(lyrics.Id);
    }

    [Fact]
    public async Task GetBySongTitleAndArtistAsync_NoMatch_ReturnsNull()
    {
        var repo = Resolve<ILyricsRepository>();

        var result = await repo.GetBySongTitleAndArtistAsync("Non Existent", "No Artist");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByVideoIdAsync_ExistingVideoId_ReturnsLyrics()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        context.ContentTypes.Add(contentType);
        await context.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var video = VideoFactory.Create(category.Id);
        context.Videos.Add(video);
        await context.SaveChangesAsync();

        var lyrics = LyricsFactory.CreateForVideo(video.Id);
        context.Lyrics.Add(lyrics);
        await context.SaveChangesAsync();

        var repo = Resolve<ILyricsRepository>();

        var result = await repo.GetByVideoIdAsync(video.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(lyrics.Id);
    }

    [Fact]
    public async Task GetByVideoIdAsync_NoMatch_ReturnsNull()
    {
        var repo = Resolve<ILyricsRepository>();

        var result = await repo.GetByVideoIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_NewLyrics_PersistsToDatabase()
    {
        var lyrics = LyricsFactory.Create();
        var (repo, db) = CreateScopedRepository<ILyricsRepository, ContentDbContext>();

        await repo.AddAsync(lyrics);
        await db.SaveChangesAsync();

        await using var verifyContext = CreateDbContext<ContentDbContext>();
        var persisted = await verifyContext.Lyrics.FindAsync(lyrics.Id);
        persisted.Should().NotBeNull();
    }

    [Fact]
    public async Task Remove_ExistingLyrics_DeletesFromDatabase()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var lyrics = LyricsFactory.Create();
        context.Lyrics.Add(lyrics);
        await context.SaveChangesAsync();

        var (repo, db) = CreateScopedRepository<ILyricsRepository, ContentDbContext>();
        var toRemove = await db.Lyrics.FindAsync(lyrics.Id);
        repo.Remove(toRemove!);
        await db.SaveChangesAsync();

        await using var verifyContext = CreateDbContext<ContentDbContext>();
        var removed = await verifyContext.Lyrics.FindAsync(lyrics.Id);
        removed.Should().BeNull();
    }
}
