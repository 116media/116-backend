using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;
using MapsterMapper;
using ContentMappingRegistration = _116.Content.Application.Shared.Mappers.MappingRegistration;

namespace _116.Integration.Tests.Modules.Content.Mappers;

/// <summary>
/// Integration tests for <see cref="PackageMapper" />.
/// Verifies entity-to-DTO mapping with slots and calculated pricing from PostgreSQL.
/// </summary>
[Collection("Database")]
public class PackageMapperTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    private readonly IMapper _mapper = new Mapper(ContentMappingRegistration.CreateConfiguration());

    [Fact]
    public async Task ToPackageDto_ShouldMapAllFields()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var package = PackageFactory.Create("Gold Package");
        seedContext.Packages.Add(package);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<ContentDbContext>();
        PackageEntity loaded = await readContext
            .Packages.Include(p => p.Slots)
                .ThenInclude(s => s.Category)
                    .ThenInclude(c => c!.Pricing)
            .FirstAsync(p => p.Id == package.Id);

        PackageDto dto = loaded.ToPackageDto(_mapper);

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

        var slot = PackageSlotFactory.Create(package.Id, category.Id);
        seedContext.PackageSlots.Add(slot);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<ContentDbContext>();
        PackageEntity loaded = await readContext
            .Packages.Include(p => p.Slots)
                .ThenInclude(s => s.Category)
                    .ThenInclude(c => c!.Pricing)
            .FirstAsync(p => p.Id == package.Id);

        PackageDto dto = loaded.ToPackageDto(_mapper);

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

        var categoryPricing = CategoryPricingFactory.Create(category.Id, pricingTier.Id, 50.00m);
        seedContext.CategoryPricing.Add(categoryPricing);

        var package = PackageFactory.Create("Premium");
        seedContext.Packages.Add(package);
        await seedContext.SaveChangesAsync();

        var slot = PackageSlotFactory.Create(package.Id, category.Id, true, 2);
        seedContext.PackageSlots.Add(slot);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<ContentDbContext>();
        PackageEntity loaded = await readContext
            .Packages.Include(p => p.Slots)
                .ThenInclude(s => s.Category)
                    .ThenInclude(c => c!.Pricing)
            .FirstAsync(p => p.Id == package.Id);

        PackageDto dto = loaded.ToPackageDto(_mapper);

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
        List<PackageEntity> loaded = await readContext
            .Packages.Include(p => p.Slots)
                .ThenInclude(s => s.Category)
                    .ThenInclude(c => c!.Pricing)
            .ToListAsync();

        IReadOnlyList<PackageDto> dtos = loaded.ToPackageDtos(_mapper);

        dtos.Should().HaveCount(2);
        dtos.Select(d => d.Name).Should().BeEquivalentTo(["Silver", "Gold"]);
    }
}
