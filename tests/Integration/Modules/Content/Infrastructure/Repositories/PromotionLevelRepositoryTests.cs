using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Infrastructure.Repositories;

/// <summary>
/// Integration tests for <see cref="IPromotionLevelRepository"/> verifying persistence behavior against a real
/// PostgreSQL database.
/// </summary>
[Collection("Database")]
public class PromotionLevelRepositoryTests : BaseRepositoryTest
{
    public PromotionLevelRepositoryTests(PostgresFixture postgres)
        : base(postgres) { }

    [Fact]
    public async Task ExistsByNameAsync_WhenExists_ReturnsTrue()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var level = PromotionLevelFactory.Create("Featured", 30, 49.99m);
        seedContext.PromotionLevels.Add(level);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IPromotionLevelRepository>();

        var exists = await repo.ExistsByNameAsync("Featured");

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByNameAsync_WithDifferentCase_ReturnsTrue()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        seedContext.PromotionLevels.Add(PromotionLevelFactory.Create("Spotlight", 30, 49.99m));
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IPromotionLevelRepository>();

        var exists = await repo.ExistsByNameAsync("sPoTlIgHt");

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByNameAsync_WhenNotFound_ReturnsFalse()
    {
        var repo = Resolve<IPromotionLevelRepository>();

        var exists = await repo.ExistsByNameAsync($"missing-{Guid.NewGuid():N}");

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllAsync_WithSearch_FiltersResults()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        seedContext.PromotionLevels.AddRange(
            PromotionLevelFactory.Create("HomepageBoost", 30, 49.99m),
            PromotionLevelFactory.Create("SidebarSlot", 7, 9.99m)
        );
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IPromotionLevelRepository>();

        var result = await repo.GetAllAsync(search: "homepage");

        result.Should().ContainSingle();
        result[0].Name.Should().Be("HomepageBoost");
    }

    [Fact]
    public async Task GetAllAsync_WithoutSearch_ReturnsAllOrderedByName()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        seedContext.PromotionLevels.AddRange(
            PromotionLevelFactory.Create("Zenith", 30, 49.99m),
            PromotionLevelFactory.Create("Anchor", 7, 9.99m)
        );
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IPromotionLevelRepository>();

        var result = await repo.GetAllAsync();

        result.Should().HaveCountGreaterThanOrEqualTo(2);
        result.Should().BeInAscendingOrder(x => x.Name);
    }

    [Fact]
    public async Task GetActiveAsync_ReturnsOnlyActiveEntities()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var active = PromotionLevelFactory.Create("ActivePromo", 7, 9.99m);
        var inactive = PromotionLevelFactory.CreateInactive();
        seedContext.PromotionLevels.AddRange(active, inactive);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IPromotionLevelRepository>();

        var result = await repo.GetActiveAsync();

        result.Should().OnlyContain(x => x.IsActive);
        result.Should().Contain(x => x.Id == active.Id);
        result.Should().NotContain(x => x.Id == inactive.Id);
    }
}
