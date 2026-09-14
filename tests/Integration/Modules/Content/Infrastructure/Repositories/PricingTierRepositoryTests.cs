using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Infrastructure.Repositories;

/// <summary>
/// Integration tests for <see cref="IPricingTierRepository"/> verifying persistence behavior against a real
/// PostgreSQL database.
/// </summary>
[Collection("Database")]
public class PricingTierRepositoryTests : BaseRepositoryTest
{
    public PricingTierRepositoryTests(PostgresFixture postgres)
        : base(postgres) { }

    [Fact]
    public async Task GetByIdOrThrowAsync_WhenExists_ReturnsEntity()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var tier = PricingTierFactory.Create("Premium");
        seedContext.PricingTiers.Add(tier);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IPricingTierRepository>();

        var result = await repo.GetByIdOrThrowAsync(tier.Id);

        result.Should().NotBeNull();
        result.Id.Should().Be(tier.Id);
        result.Name.Should().Be("Premium");
    }

    [Fact]
    public async Task GetByIdOrThrowAsync_WhenNotFound_ThrowsNotFoundException()
    {
        var repo = Resolve<IPricingTierRepository>();

        var act = () => repo.GetByIdOrThrowAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ExistsByNameAsync_WithDifferentCase_ReturnsTrue()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        seedContext.PricingTiers.Add(PricingTierFactory.Create("Platinum"));
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IPricingTierRepository>();

        var exists = await repo.ExistsByNameAsync("pLaTiNuM");

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByNameAsync_WhenNotFound_ReturnsFalse()
    {
        var repo = Resolve<IPricingTierRepository>();

        var exists = await repo.ExistsByNameAsync($"missing-{Guid.NewGuid():N}");

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllAsync_WithoutSearch_ReturnsAllOrderedByName()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        seedContext.PricingTiers.AddRange(PricingTierFactory.Create("Zinc"), PricingTierFactory.Create("Amber"));
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IPricingTierRepository>();

        var result = await repo.GetAllAsync();

        result.Should().HaveCountGreaterThanOrEqualTo(2);
        result.Should().BeInAscendingOrder(x => x.Name);
    }

    [Fact]
    public async Task GetAllAsync_WithSearch_FiltersResults()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        seedContext.PricingTiers.AddRange(
            PricingTierFactory.Create("GoldTier"),
            PricingTierFactory.Create("SilverTier"),
            PricingTierFactory.Create("Bronze")
        );
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IPricingTierRepository>();

        var result = await repo.GetAllAsync(search: "Gold");

        result.Should().ContainSingle();
        result[0].Name.Should().Be("GoldTier");
    }
}
