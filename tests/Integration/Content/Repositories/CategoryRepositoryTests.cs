using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;

namespace _116.Integration.Tests.Content.Repositories;

/// <summary>
/// Integration tests for <see cref="ICategoryRepository"/> verifying persistence behavior
/// for categories and category pricing against a real PostgreSQL database.
/// </summary>
[Collection("Database")]
public class CategoryRepositoryTests : BaseRepositoryTest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CategoryRepositoryTests"/> class
    /// with the shared database fixture.
    /// </summary>
    public CategoryRepositoryTests(PostgresFixture postgres)
        : base(postgres) { }

    [Fact]
    public async Task GetAllAsync_ReturnsPaginatedResults()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        var categories = CategoryFactory.CreateMany(contentType.Id, 5);
        seedContext.Categories.AddRange(categories);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<ICategoryRepository>();
        var (result, totalCount) = await repo.GetAllAsync(page: 1, pageSize: 3, isActive: null, isFree: null);

        totalCount.Should().BeGreaterThanOrEqualTo(5);
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllAsync_WithIsActiveFilter_ReturnsOnlyActiveCategories()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        var active = CategoryFactory.Create(contentType.Id);
        var inactive = CategoryFactory.CreateInactive(contentType.Id);
        seedContext.Categories.AddRange(active, inactive);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<ICategoryRepository>();
        var (result, _) = await repo.GetAllAsync(page: 1, pageSize: 100, isActive: true, isFree: null);

        result.Should().OnlyContain(c => c.IsActive);
    }

    [Fact]
    public async Task GetAllAsync_WithIsFreeFilter_ReturnsOnlyFreeCategories()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        var free = CategoryFactory.CreateFree(contentType.Id);
        var paid = CategoryFactory.CreatePaid(contentType.Id);
        seedContext.Categories.AddRange(free, paid);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<ICategoryRepository>();
        var (result, _) = await repo.GetAllAsync(page: 1, pageSize: 100, isActive: null, isFree: true);

        result.Should().OnlyContain(c => c.IsFree);
    }

    [Fact]
    public async Task GetAllAsync_WithIsPaidFilter_ReturnsOnlyPaidCategories()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        var free = CategoryFactory.CreateFree(contentType.Id);
        var paid = CategoryFactory.CreatePaid(contentType.Id);
        seedContext.Categories.AddRange(free, paid);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<ICategoryRepository>();
        var (result, _) = await repo.GetAllAsync(page: 1, pageSize: 100, isActive: null, isFree: false);

        result.Should().OnlyContain(c => !c.IsFree);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCategoryExists_ReturnsEntity()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<ICategoryRepository>();
        var result = await repo.GetByIdAsync(category.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(category.Id);
        result.ContentType.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WhenCategoryDoesNotExist_ReturnsNull()
    {
        var repo = Resolve<ICategoryRepository>();
        var result = await repo.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdOrThrowAsync_WhenCategoryExists_ReturnsEntity()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<ICategoryRepository>();
        var result = await repo.GetByIdOrThrowAsync(category.Id);

        result.Should().NotBeNull();
        result.Id.Should().Be(category.Id);
    }

    [Fact]
    public async Task GetByIdOrThrowAsync_WhenCategoryDoesNotExist_ThrowsNotFoundException()
    {
        var repo = Resolve<ICategoryRepository>();

        var act = () => repo.GetByIdOrThrowAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetBySlugAsync_WhenSlugExists_ReturnsEntity()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id, "Test Slug Category", "test-slug-category");
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<ICategoryRepository>();
        var result = await repo.GetBySlugAsync("test-slug-category");

        result.Should().NotBeNull();
        result!.Slug.Should().Be("test-slug-category");
    }

    [Fact]
    public async Task GetBySlugAsync_WhenSlugDoesNotExist_ReturnsNull()
    {
        var repo = Resolve<ICategoryRepository>();
        var result = await repo.GetBySlugAsync("non-existent-slug");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetActiveByContentTypeAsync_ReturnsOnlyActiveCategories()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        var active = CategoryFactory.Create(contentType.Id);
        var inactive = CategoryFactory.CreateInactive(contentType.Id);
        seedContext.Categories.AddRange(active, inactive);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<ICategoryRepository>();
        var result = await repo.GetActiveByContentTypeAsync(contentTypeId: null);

        result.Should().OnlyContain(c => c.IsActive);
    }

    [Fact]
    public async Task GetActiveByContentTypeAsync_WithContentTypeId_FiltersbyContentType()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType1 = ContentTypeFactory.Create();
        var contentType2 = ContentTypeFactory.Create();
        seedContext.ContentTypes.AddRange(contentType1, contentType2);
        await seedContext.SaveChangesAsync();

        var cat1 = CategoryFactory.Create(contentType1.Id);
        var cat2 = CategoryFactory.Create(contentType2.Id);
        seedContext.Categories.AddRange(cat1, cat2);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<ICategoryRepository>();
        var result = await repo.GetActiveByContentTypeAsync(contentType1.Id);

        result.Should().OnlyContain(c => c.ContentTypeId == contentType1.Id);
    }

    [Fact]
    public async Task AddAsync_PersistsCategoryToDatabase()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        var (repo, db) = CreateScopedRepository<ICategoryRepository, ContentDbContext>();
        var category = CategoryFactory.Create(contentType.Id);

        await repo.AddAsync(category);
        await db.SaveChangesAsync();

        await using var verifyContext = CreateDbContext<ContentDbContext>();
        var persisted = await verifyContext.Categories.FirstOrDefaultAsync(c => c.Id == category.Id);

        persisted.Should().NotBeNull();
        persisted!.Name.Should().Be(category.Name);
    }

    [Fact]
    public async Task GetPricingByCategoryAsync_ReturnsPricingForCategory()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        var category = CategoryFactory.CreatePaid(contentType.Id);
        seedContext.Categories.Add(category);

        var tier = PricingTierFactory.Create();
        seedContext.PricingTiers.Add(tier);
        await seedContext.SaveChangesAsync();

        var pricing = CategoryPricingFactory.Create(category.Id, tier.Id);
        seedContext.CategoryPricing.Add(pricing);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<ICategoryRepository>();
        var result = await repo.GetPricingByCategoryAsync(category.Id);

        result.Should().ContainSingle();
        result[0].CategoryId.Should().Be(category.Id);
        result[0].PricingTierId.Should().Be(tier.Id);
    }

    [Fact]
    public async Task GetPricingAsync_WhenExists_ReturnsPricingEntity()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        var category = CategoryFactory.CreatePaid(contentType.Id);
        seedContext.Categories.Add(category);

        var tier = PricingTierFactory.Create();
        seedContext.PricingTiers.Add(tier);
        await seedContext.SaveChangesAsync();

        var pricing = CategoryPricingFactory.Create(category.Id, tier.Id);
        seedContext.CategoryPricing.Add(pricing);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<ICategoryRepository>();
        var result = await repo.GetPricingAsync(category.Id, tier.Id);

        result.Should().NotBeNull();
        result!.CategoryId.Should().Be(category.Id);
        result.PricingTierId.Should().Be(tier.Id);
    }

    [Fact]
    public async Task GetPricingAsync_WhenDoesNotExist_ReturnsNull()
    {
        var repo = Resolve<ICategoryRepository>();
        var result = await repo.GetPricingAsync(Guid.NewGuid(), Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task AddPricingAsync_PersistsPricingToDatabase()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        var category = CategoryFactory.CreatePaid(contentType.Id);
        seedContext.Categories.Add(category);

        var tier = PricingTierFactory.Create();
        seedContext.PricingTiers.Add(tier);
        await seedContext.SaveChangesAsync();

        var (repo, db) = CreateScopedRepository<ICategoryRepository, ContentDbContext>();
        var pricing = CategoryPricingFactory.Create(category.Id, tier.Id);

        await repo.AddPricingAsync(pricing);
        await db.SaveChangesAsync();

        await using var verifyContext = CreateDbContext<ContentDbContext>();
        var persisted = await verifyContext.CategoryPricing.FirstOrDefaultAsync(p =>
            p.CategoryId == category.Id && p.PricingTierId == tier.Id
        );

        persisted.Should().NotBeNull();
    }

    [Fact]
    public async Task RemovePricing_DeletesPricingFromDatabase()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        var category = CategoryFactory.CreatePaid(contentType.Id);
        seedContext.Categories.Add(category);

        var tier = PricingTierFactory.Create();
        seedContext.PricingTiers.Add(tier);
        await seedContext.SaveChangesAsync();

        var pricing = CategoryPricingFactory.Create(category.Id, tier.Id);
        seedContext.CategoryPricing.Add(pricing);
        await seedContext.SaveChangesAsync();

        var (repo, db) = CreateScopedRepository<ICategoryRepository, ContentDbContext>();
        var existing = await db.CategoryPricing.FirstAsync(p =>
            p.CategoryId == category.Id && p.PricingTierId == tier.Id
        );

        repo.RemovePricing(existing);
        await db.SaveChangesAsync();

        await using var verifyContext = CreateDbContext<ContentDbContext>();
        var deleted = await verifyContext.CategoryPricing.FirstOrDefaultAsync(p =>
            p.CategoryId == category.Id && p.PricingTierId == tier.Id
        );

        deleted.Should().BeNull();
    }

    [Fact]
    public async Task GetGossipCategoryAsync_WhenGossipExists_ReturnsEntity()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id, "Gossip Category", "gossip-category");
        category.Update(
            category.Name,
            category.Slug,
            "Gossip description",
            isGossip: true,
            isExclusive: false,
            TestErrorsFactory.CreateCategoryErrors()
        );
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<ICategoryRepository>();
        var result = await repo.GetGossipCategoryAsync();

        result.Should().NotBeNull();
        result!.Id.Should().Be(category.Id);
    }

    [Fact]
    public async Task GetGossipCategoryAsync_WhenNoGossipExists_ReturnsNull()
    {
        var repo = Resolve<ICategoryRepository>();
        var result = await repo.GetGossipCategoryAsync();

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetExclusiveCategoryAsync_WhenExclusiveExists_ReturnsEntity()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id, isExclusive: true);
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<ICategoryRepository>();
        var result = await repo.GetExclusiveCategoryAsync();

        result.Should().NotBeNull();
        result!.Id.Should().Be(category.Id);
    }

    [Fact]
    public async Task GetExclusiveCategoryAsync_WhenNoExclusiveExists_ReturnsNull()
    {
        var repo = Resolve<ICategoryRepository>();
        var result = await repo.GetExclusiveCategoryAsync();

        result.Should().BeNull();
    }
}
