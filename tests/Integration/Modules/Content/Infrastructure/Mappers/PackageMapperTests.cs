using _116.Content.Application.Catalog.Factories;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Mappers;

/// <summary>
/// Integration tests for <see cref="PackageMapper" /> through <see cref="IPackageDtoFactory" />.
/// Verifies that the slot category names and the calculated price come from the factory's
/// batched category lookup rather than from a navigation.
/// </summary>
[Collection("Database")]
public class PackageMapperTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    [Fact]
    public async Task ToPackageDto_ShouldMapAllFields()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var package = PackageFactory.Create("Gold Package");
        seedContext.Packages.Add(package);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<ContentDbContext>();
        PackageEntity loaded = await readContext.Packages.Include(p => p.Slots).FirstAsync(p => p.Id == package.Id);

        PackageDto dto = await Resolve<IPackageDtoFactory>().CreateAsync(loaded);

        dto.Id.Should().Be(loaded.Id);
        dto.Name.Should().Be("Gold Package");
        dto.Description.Should().Be(loaded.Description);
        dto.IsActive.Should().Be(loaded.IsActive);
    }

    [Fact]
    public async Task ToPackageDto_WithSlots_ShouldMapSlotCollection()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create("Video");
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id, "Music", "music");
        seedContext.Categories.Add(category);

        var package = PackageFactory.Create("Standard");
        seedContext.Packages.Add(package);
        await seedContext.SaveChangesAsync();

        var slot = PackageSlotFactory.Create(package, category.Id);
        seedContext.PackageSlots.Add(slot);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<ContentDbContext>();
        PackageEntity loaded = await readContext.Packages.Include(p => p.Slots).FirstAsync(p => p.Id == package.Id);

        PackageDto dto = await Resolve<IPackageDtoFactory>().CreateAsync(loaded);

        dto.Slots.Should().ContainSingle();
        dto.Slots[0].CategoryName.Should().Be("Music");
        dto.Slots[0].CategoryId.Should().Be(category.Id);
    }

    [Fact]
    public async Task ToPackageDto_WithRequiredSlotsAndPricing_ShouldCalculatePrice()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create("Video");
        seedContext.ContentTypes.Add(contentType);

        var pricingTier = PricingTierFactory.Create("base");
        seedContext.PricingTiers.Add(pricingTier);
        await seedContext.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id, "Music", "music");
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        var categoryPricing = CategoryPricingFactory.Create(category, pricingTier.Id, 50.00m);
        seedContext.CategoryPricing.Add(categoryPricing);

        var package = PackageFactory.Create("Premium");
        seedContext.Packages.Add(package);
        await seedContext.SaveChangesAsync();

        var slot = PackageSlotFactory.Create(package, category.Id, true, 2);
        seedContext.PackageSlots.Add(slot);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<ContentDbContext>();
        PackageEntity loaded = await readContext.Packages.Include(p => p.Slots).FirstAsync(p => p.Id == package.Id);

        PackageDto dto = await Resolve<IPackageDtoFactory>().CreateAsync(loaded);

        dto.CalculatedPriceUsd.Should().Be(100.00m);
    }

    [Fact]
    public async Task ToPackageDtos_ShouldMapCollection()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var p1 = PackageFactory.Create("Silver");
        var p2 = PackageFactory.Create("Gold");
        seedContext.Packages.AddRange(p1, p2);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<ContentDbContext>();
        List<PackageEntity> loaded = await readContext.Packages.Include(p => p.Slots).ToListAsync();

        IReadOnlyList<PackageDto> dtos = await Resolve<IPackageDtoFactory>().CreateManyAsync(loaded);

        dtos.Should().HaveCount(2);
        dtos.Select(d => d.Name).Should().BeEquivalentTo(["Silver", "Gold"]);
    }
}
