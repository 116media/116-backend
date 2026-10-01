using _116.Content.Application.Catalog.Services;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Content.TestData.Factories;

namespace _116.Content.Integration.Tests.Application.Catalog.Services;

/// <summary>
/// Integration tests for <see cref="ICategoryDtoService" />, resolved from the container.
/// Verifies entity-to-DTO projection with navigation properties loaded from PostgreSQL.
/// </summary>
[Collection("Database")]
public class CategoryDtoServiceTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    [Fact]
    public async Task CreateAsync_ShouldMapAllFields()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create("Video");
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<ContentDbContext>();
        CategoryEntity loaded = await readContext
            .Categories.Include(c => c.Pricing)
            .FirstAsync(c => c.Id == category.Id);

        var categoryDtoService = Resolve<ICategoryDtoService>();
        CategoryDto dto = await categoryDtoService.CreateAsync(loaded);

        dto.Id.Should().Be(loaded.Id);
        dto.Name.Should().Be(loaded.Name);
        dto.Slug.Should().Be(loaded.Slug);
        dto.Description.Should().Be(loaded.Description);
        dto.IsActive.Should().Be(loaded.IsActive);
        dto.ContentTypeId.Should().Be(contentType.Id);
        dto.ContentTypeName.Should().Be("Video");
        dto.IsFree.Should().Be(loaded.IsFree);
        dto.IsExclusive.Should().Be(loaded.IsExclusive);
    }

    [Fact]
    public async Task CreateAsync_WithNullPoster_ShouldMapPosterUrlAsNull()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create("Article");
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<ContentDbContext>();
        CategoryEntity loaded = await readContext
            .Categories.Include(c => c.Pricing)
            .FirstAsync(c => c.Id == category.Id);

        var categoryDtoService = Resolve<ICategoryDtoService>();
        CategoryDto dto = await categoryDtoService.CreateAsync(loaded);

        dto.PosterUrl.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_WithPricing_ShouldMapPricingCollection()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create("Video");
        seedContext.ContentTypes.Add(contentType);

        var pricingTier = PricingTierFactory.Create("base_upload");
        seedContext.PricingTiers.Add(pricingTier);
        await seedContext.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        var pricing = CategoryPricingFactory.Create(category, pricingTier.Id, 25.00m);
        seedContext.CategoryPricing.Add(pricing);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<ContentDbContext>();
        CategoryEntity loaded = await readContext
            .Categories.Include(c => c.Pricing)
            .FirstAsync(c => c.Id == category.Id);

        var categoryDtoService = Resolve<ICategoryDtoService>();
        CategoryDto dto = await categoryDtoService.CreateAsync(loaded);

        dto.Pricing.Should().ContainSingle();
        dto.Pricing[0].TierName.Should().Be("base_upload");
        dto.Pricing[0].PriceUsd.Should().Be(25.00m);
    }

    [Fact]
    public async Task CreateManyAsync_ShouldMapCollection()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create("Video");
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        var cat1 = CategoryFactory.Create(contentType.Id, "Music", "music");
        var cat2 = CategoryFactory.Create(contentType.Id, "Culture", "culture");
        seedContext.Categories.AddRange(cat1, cat2);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<ContentDbContext>();
        List<CategoryEntity> loaded = await readContext.Categories.Include(c => c.Pricing).ToListAsync();

        var categoryDtoService = Resolve<ICategoryDtoService>();
        IReadOnlyList<CategoryDto> dtos = await categoryDtoService.CreateManyAsync(loaded);

        dtos.Should().HaveCount(2);
        dtos.Select(d => d.Name).Should().BeEquivalentTo(["Music", "Culture"]);
    }
}
