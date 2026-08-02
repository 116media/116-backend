using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Content.Infrastructure.Persistence.Seeds.ContentTypes;

namespace _116.Integration.Tests.Modules.Content.Seeders;

/// <summary>
/// Integration tests for <see cref="ContentTypeSeeder" />.
/// Verifies content type seeding against real PostgreSQL.
/// </summary>
[Collection("Database")]
public class ContentTypeSeederTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    [Fact]
    public async Task SeedAllAsync_ShouldCreateAllContentTypes()
    {
        var seeder = Resolve<ContentTypeSeeder>();

        await seeder.SeedAllAsync();

        await using var context = CreateDbContext<ContentDbContext>();
        List<ContentTypeEntity> types = await context.ContentTypes.ToListAsync();

        string[] expectedNames =
        [
            nameof(EnumCoreContentType.Article),
            nameof(EnumCoreContentType.Video),
            nameof(EnumCoreContentType.Short),
            nameof(EnumCoreContentType.Lyrics),
        ];
        types.Should().HaveCount(expectedNames.Length);
        types.Select(t => t.Name).Should().BeEquivalentTo(expectedNames);
    }

    [Fact]
    public async Task SeedAllAsync_CalledTwice_ShouldBeIdempotent()
    {
        var seeder = Resolve<ContentTypeSeeder>();

        await seeder.SeedAllAsync();
        await seeder.SeedAllAsync();

        await using var context = CreateDbContext<ContentDbContext>();
        int count = await context.ContentTypes.CountAsync();

        count.Should().Be(4);
    }

    [Fact]
    public async Task SeedAllAsync_ShouldCreateActiveContentTypes()
    {
        var seeder = Resolve<ContentTypeSeeder>();

        await seeder.SeedAllAsync();

        await using var context = CreateDbContext<ContentDbContext>();
        List<ContentTypeEntity> types = await context.ContentTypes.ToListAsync();

        types.Should().AllSatisfy(t => t.IsActive.Should().BeTrue());
    }

    [Fact]
    public async Task SeedAllAsync_ShouldAssignUniqueIds()
    {
        var seeder = Resolve<ContentTypeSeeder>();

        await seeder.SeedAllAsync();

        await using var context = CreateDbContext<ContentDbContext>();
        List<ContentTypeEntity> types = await context.ContentTypes.ToListAsync();

        types.Should().OnlyHaveUniqueItems(t => t.Id);
    }
}
