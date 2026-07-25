using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Infrastructure.Repositories;

/// <summary>
/// Integration tests for <see cref="IArtistRepository" /> verifying artist CRUD, lookup, and
/// deletion-cascade behavior against a real PostgreSQL database.
/// </summary>
[Collection("Database")]
public class ArtistRepositoryTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    [Fact]
    public async Task GetBySlugAsync_ExistingMatch_ReturnsArtist()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        string slug = $"unique-artist-slug-{Guid.NewGuid():N}";
        var artist = ArtistFactory.CreateWithSlug(slug);
        context.Artists.Add(artist);
        await context.SaveChangesAsync();

        var repo = Resolve<IArtistRepository>();

        var result = await repo.GetBySlugAsync(slug);

        result.Should().NotBeNull();
        result!.Id.Should().Be(artist.Id);
    }

    [Fact]
    public async Task GetBySlugAsync_NoMatch_ReturnsNull()
    {
        var repo = Resolve<IArtistRepository>();

        var result = await repo.GetBySlugAsync($"non-existent-slug-{Guid.NewGuid():N}");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ExistingArtist_ReturnsArtist()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var artist = ArtistFactory.Create();
        context.Artists.Add(artist);
        await context.SaveChangesAsync();

        var repo = Resolve<IArtistRepository>();

        var result = await repo.GetByIdAsync(artist.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(artist.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentArtist_ReturnsNull()
    {
        var repo = Resolve<IArtistRepository>();

        var result = await repo.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdOrThrowAsync_NonExistentArtist_ThrowsException()
    {
        var repo = Resolve<IArtistRepository>();

        var act = async () => await repo.GetByIdOrThrowAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task GetByUserIdAsync_ClaimedArtist_ReturnsArtist()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        Guid userId = Guid.NewGuid();
        var artist = ArtistFactory.CreateClaimed(userId);
        context.Artists.Add(artist);
        await context.SaveChangesAsync();

        var repo = Resolve<IArtistRepository>();

        var result = await repo.GetByUserIdAsync(userId);

        result.Should().NotBeNull();
        result!.Id.Should().Be(artist.Id);
    }

    [Fact]
    public async Task GetByUserIdAsync_UnclaimedArtist_ReturnsNull()
    {
        var repo = Resolve<IArtistRepository>();

        var result = await repo.GetByUserIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_NewArtist_PersistsToDatabase()
    {
        var artist = ArtistFactory.Create();
        var (repo, db) = CreateScopedRepository<IArtistRepository, ContentDbContext>();

        await repo.AddAsync(artist);
        await db.SaveChangesAsync();

        await using var verifyContext = CreateDbContext<ContentDbContext>();
        var persisted = await verifyContext.Artists.FindAsync(artist.Id);
        persisted.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllAsync_WithSearchQuery_ReturnsFilteredResults()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        string uniqueName = $"UniqueArtistKw{Guid.NewGuid():N}"[..20];
        var matchingArtist = ArtistFactory.Create(uniqueName, $"slug-{Guid.NewGuid():N}");
        var nonMatchingArtist = ArtistFactory.Create();
        context.Artists.AddRange(matchingArtist, nonMatchingArtist);
        await context.SaveChangesAsync();

        var repo = Resolve<IArtistRepository>();
        var (result, totalCount) = await repo.GetAllAsync(1, 100, uniqueName);

        totalCount.Should().BeGreaterThanOrEqualTo(1);
        result.Should().Contain(a => a.Id == matchingArtist.Id);
        result.Should().NotContain(a => a.Id == nonMatchingArtist.Id);
    }

    /// <summary>
    /// Exercises the <c>ArtistId</c> FK's <c>OnDelete(DeleteBehavior.SetNull)</c> configuration
    /// end-to-end against real Postgres: deleting a claimed artist with linked lyrics and videos
    /// must set their <c>ArtistId</c> to null rather than cascading the delete or throwing an FK
    /// violation.
    /// </summary>
    [Fact]
    public async Task DeletingClaimedArtistWithLinkedContent_SetsArtistIdNullOnLyricsAndVideos_WithoutCascadingOrThrowing()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        context.ContentTypes.Add(contentType);
        await context.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var artist = ArtistFactory.CreateClaimed(Guid.NewGuid());
        context.Artists.Add(artist);
        await context.SaveChangesAsync();

        var lyrics = LyricsFactory.CreateForArtist(category.Id, artist.Id);
        var video = VideoFactory.CreateForArtist(category.Id, artist.Id);
        context.Lyrics.Add(lyrics);
        context.Videos.Add(video);
        await context.SaveChangesAsync();

        await using (var deleteContext = CreateDbContext<ContentDbContext>())
        {
            var artistToDelete = await deleteContext.Artists.FindAsync(artist.Id);
            deleteContext.Artists.Remove(artistToDelete!);
            Func<Task> act = async () => await deleteContext.SaveChangesAsync();
            await act.Should().NotThrowAsync();
        }

        await using var verifyContext = CreateDbContext<ContentDbContext>();
        var persistedArtist = await verifyContext.Artists.FindAsync(artist.Id);
        var persistedLyrics = await verifyContext.Lyrics.FindAsync(lyrics.Id);
        var persistedVideo = await verifyContext.Videos.FindAsync(video.Id);

        persistedArtist.Should().BeNull();
        persistedLyrics.Should().NotBeNull();
        persistedLyrics!.ArtistId.Should().BeNull();
        persistedVideo.Should().NotBeNull();
        persistedVideo!.ArtistId.Should().BeNull();
    }
}
