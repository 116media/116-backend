using _116.Content.Application.Shared.Repositories;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Infrastructure.Repositories;

/// <summary>
/// Integration tests for <see cref="IAlbumRepository" /> verifying album CRUD, search, and
/// deletion-cascade behavior against a real PostgreSQL database.
/// </summary>
[Collection("Database")]
public class AlbumRepositoryTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    [Fact]
    public async Task GetByIdAsync_ExistingAlbum_ReturnsAlbum()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var album = AlbumFactory.Create();
        context.Albums.Add(album);
        await context.SaveChangesAsync();

        var repo = Resolve<IAlbumRepository>();

        var result = await repo.GetByIdAsync(album.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(album.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentAlbum_ReturnsNull()
    {
        var repo = Resolve<IAlbumRepository>();

        var result = await repo.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdOrThrowAsync_NonExistentAlbum_ThrowsException()
    {
        var repo = Resolve<IAlbumRepository>();

        var act = async () => await repo.GetByIdOrThrowAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task AddAsync_NewAlbum_PersistsToDatabase()
    {
        var album = AlbumFactory.Create();
        var (repo, db) = CreateScopedRepository<IAlbumRepository, ContentDbContext>();

        await repo.AddAsync(album);
        await db.SaveChangesAsync();

        await using var verifyContext = CreateDbContext<ContentDbContext>();
        var persisted = await verifyContext.Albums.FindAsync(album.Id);
        persisted.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllAsync_WithSearchQuery_ReturnsFilteredResults()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        string uniqueName = $"UniqueAlbumKw{Guid.NewGuid():N}"[..20];
        var matchingAlbum = AlbumFactory.CreateWithName(uniqueName);
        var nonMatchingAlbum = AlbumFactory.Create();
        context.Albums.AddRange(matchingAlbum, nonMatchingAlbum);
        await context.SaveChangesAsync();

        var repo = Resolve<IAlbumRepository>();
        var (result, totalCount) = await repo.GetAllAsync(1, 100, uniqueName);

        totalCount.Should().BeGreaterThanOrEqualTo(1);
        result.Should().Contain(a => a.Id == matchingAlbum.Id);
        result.Should().NotContain(a => a.Id == nonMatchingAlbum.Id);
    }

    /// <summary>
    /// Exercises the album's own <c>ArtistId</c> FK <c>OnDelete(DeleteBehavior.SetNull)</c>
    /// configuration: deleting the linked artist must set the album's <c>ArtistId</c> to null
    /// rather than cascading the delete or throwing an FK violation.
    /// </summary>
    [Fact]
    public async Task DeletingLinkedArtist_SetsAlbumArtistIdNull_WithoutCascadingOrThrowing()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var artist = ArtistFactory.Create();
        context.Artists.Add(artist);
        await context.SaveChangesAsync();

        var album = AlbumFactory.CreateForArtist(artist.Id);
        context.Albums.Add(album);
        await context.SaveChangesAsync();

        await using (var deleteContext = CreateDbContext<ContentDbContext>())
        {
            var artistToDelete = await deleteContext.Artists.FindAsync(artist.Id);
            deleteContext.Artists.Remove(artistToDelete!);
            Func<Task> act = async () => await deleteContext.SaveChangesAsync();
            await act.Should().NotThrowAsync();
        }

        await using var verifyContext = CreateDbContext<ContentDbContext>();
        var persistedAlbum = await verifyContext.Albums.FindAsync(album.Id);
        persistedAlbum.Should().NotBeNull();
        persistedAlbum!.ArtistId.Should().BeNull();
    }
}
