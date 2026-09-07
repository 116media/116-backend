using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Infrastructure.Repositories;

/// <summary>
/// Integration tests for the open-generic <see cref="IContentRepository{TEntity}" /> registration,
/// verifying a closed instance resolves from DI and performs the common aggregate operations
/// against a real PostgreSQL database.
/// </summary>
[Collection("Database")]
public class ContentRepositoryTests : BaseRepositoryTest
{
    public ContentRepositoryTests(PostgresFixture postgres)
        : base(postgres) { }

    [Fact]
    public async Task AddAsync_WhenCommitted_PersistsAggregate()
    {
        (IContentRepository<TagEntity> repo, ContentDbContext db) = CreateScopedRepository<
            IContentRepository<TagEntity>,
            ContentDbContext
        >();
        var tag = TagFactory.Create("Gospel", "gospel");

        await repo.AddAsync(tag);
        await db.SaveChangesAsync();

        await using var verifyContext = CreateDbContext<ContentDbContext>();
        TagEntity? persisted = await verifyContext.Tags.FindAsync(tag.Id);
        persisted.Should().NotBeNull();
        persisted!.Slug.Should().Be("gospel");
    }

    [Fact]
    public async Task GetByIdAsync_WhenExists_ReturnsAggregate()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var tag = TagFactory.Create("Afrobeats", "afrobeats");
        seedContext.Tags.Add(tag);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IContentRepository<TagEntity>>();

        TagEntity? result = await repo.GetByIdAsync(tag.Id);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Afrobeats");
    }

    [Fact]
    public async Task ExistsAsync_WhenMissing_ReturnsFalse()
    {
        var repo = Resolve<IContentRepository<TagEntity>>();

        bool exists = await repo.ExistsAsync(Guid.NewGuid());

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task GetByIdOrThrowAsync_WhenMissing_ThrowsNotFoundException()
    {
        var repo = Resolve<IContentRepository<TagEntity>>();

        var act = () => repo.GetByIdOrThrowAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Remove_WhenCommitted_DeletesAggregate()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var tag = TagFactory.Create("Drill", "drill");
        seedContext.Tags.Add(tag);
        await seedContext.SaveChangesAsync();

        (IContentRepository<TagEntity> repo, ContentDbContext db) = CreateScopedRepository<
            IContentRepository<TagEntity>,
            ContentDbContext
        >();
        TagEntity tracked = await repo.GetByIdOrThrowAsync(tag.Id);

        repo.Remove(tracked);
        await db.SaveChangesAsync();

        await using var verifyContext = CreateDbContext<ContentDbContext>();
        (await verifyContext.Tags.FindAsync(tag.Id)).Should().BeNull();
    }
}
