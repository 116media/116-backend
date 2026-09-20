using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Content.Infrastructure.Repositories;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common.Helpers;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Infrastructure.Repositories;

/// <summary>
/// Unit tests for <see cref="CategoryRepository"/> using InMemory database.
/// </summary>
public class CategoryRepositoryTests : IDisposable
{
    private readonly ContentDbContext _context;
    private readonly CategoryRepository _repository;

    public CategoryRepositoryTests()
    {
        DbContextOptions<ContentDbContext> options = new DbContextOptionsBuilder<ContentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .AddInterceptors(new CreatedAtStampingInterceptor())
            .Options;

        _context = new ContentDbContext(options);
        _repository = new CategoryRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task<ContentTypeEntity> AddContentTypeAsync(string name = "Article")
    {
        ContentTypeEntity contentType = ContentTypeFactory.Create(name);
        _context.ContentTypes.Add(contentType);
        await _context.SaveChangesAsync();
        return contentType;
    }

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_ShouldPersistCategoryEntity()
    {
        // Arrange
        ContentTypeEntity contentType = await AddContentTypeAsync();
        CategoryEntity category = CategoryFactory.Create(contentType.Id);

        // Act
        await _repository.AddAsync(category);
        await _context.SaveChangesAsync();

        // Assert
        CategoryEntity? retrieved = await _context.Categories.FindAsync(category.Id);
        retrieved.Should().NotBeNull();
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenFound_ShouldReturnEntity()
    {
        // Arrange
        ContentTypeEntity contentType = await AddContentTypeAsync();
        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        // Act
        CategoryEntity? result = await _repository.GetByIdAsync(category.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(category.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ShouldReturnNull()
    {
        // Act
        CategoryEntity? result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetByIdOrThrowAsync Tests

    [Fact]
    public async Task GetByIdOrThrowAsync_WhenFound_ShouldReturnEntity()
    {
        // Arrange
        ContentTypeEntity contentType = await AddContentTypeAsync();
        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        // Act
        CategoryEntity result = await _repository.GetByIdOrThrowAsync(category.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(category.Id);
    }

    [Fact]
    public async Task GetByIdOrThrowAsync_WhenNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        Func<Task> act = async () => await _repository.GetByIdOrThrowAsync(nonExistentId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region GetBySlugAsync Tests

    [Fact]
    public async Task GetBySlugAsync_WhenNotFound_ShouldReturnNull()
    {
        // Act
        CategoryEntity? result = await _repository.GetBySlugAsync("nonexistent-slug");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_ShouldReturnPaginatedResult()
    {
        // Arrange
        ContentTypeEntity contentType = await AddContentTypeAsync();
        _context.Categories.AddRange(CategoryFactory.CreateMany(contentType.Id, 5));
        await _context.SaveChangesAsync();

        // Act
        (List<CategoryEntity> categories, int totalCount) = await _repository.GetAllAsync(
            page: 1,
            pageSize: 3,
            isActive: null,
            isFree: null
        );

        // Assert
        categories.Should().HaveCount(3);
        totalCount.Should().Be(5);
    }

    [Fact]
    public async Task GetAllAsync_WithIsActiveFilter_ShouldReturnFilteredCategories()
    {
        // Arrange
        ContentTypeEntity contentType = await AddContentTypeAsync();
        _context.Categories.Add(CategoryFactory.Create(contentType.Id));
        _context.Categories.Add(CategoryFactory.Create(contentType.Id));
        _context.Categories.Add(CategoryFactory.CreateInactive(contentType.Id));
        await _context.SaveChangesAsync();

        // Act
        (List<CategoryEntity> active, int totalCount) = await _repository.GetAllAsync(
            page: 1,
            pageSize: 10,
            isActive: true,
            isFree: null
        );

        // Assert
        active.Should().HaveCount(2);
        totalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetAllAsync_WithIsFreeFilter_ShouldReturnFilteredCategories()
    {
        // Arrange
        ContentTypeEntity contentType = await AddContentTypeAsync();
        _context.Categories.Add(CategoryFactory.CreateFree(contentType.Id));
        _context.Categories.Add(CategoryFactory.CreatePaid(contentType.Id));
        await _context.SaveChangesAsync();

        // Act
        (List<CategoryEntity> free, int totalCount) = await _repository.GetAllAsync(
            page: 1,
            pageSize: 10,
            isActive: null,
            isFree: true
        );

        // Assert
        free.Should().ContainSingle();
        free[0].IsFree.Should().BeTrue();
    }

    [Fact]
    public async Task GetAllAsync_SecondPage_ShouldReturnNextPageItems()
    {
        // Arrange
        ContentTypeEntity contentType = await AddContentTypeAsync();
        _context.Categories.AddRange(CategoryFactory.CreateMany(contentType.Id, 5));
        await _context.SaveChangesAsync();

        // Act
        (List<CategoryEntity> page2, int totalCount) = await _repository.GetAllAsync(
            page: 2,
            pageSize: 3,
            isActive: null,
            isFree: null
        );

        // Assert
        page2.Should().HaveCount(2);
        totalCount.Should().Be(5);
    }

    #endregion

    #region GetActiveByContentTypeAsync Tests

    [Fact]
    public async Task GetActiveByContentTypeAsync_WithNoFilter_ShouldReturnAllActive()
    {
        // Arrange
        ContentTypeEntity contentType = await AddContentTypeAsync();
        _context.Categories.Add(CategoryFactory.Create(contentType.Id));
        _context.Categories.Add(CategoryFactory.Create(contentType.Id));
        _context.Categories.Add(CategoryFactory.CreateInactive(contentType.Id));
        await _context.SaveChangesAsync();

        // Act
        IReadOnlyList<CategoryEntity> result = await _repository.GetActiveByContentTypeAsync(null);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetActiveByContentTypeAsync_WithContentTypeId_ShouldReturnFiltered()
    {
        // Arrange
        ContentTypeEntity contentType1 = await AddContentTypeAsync("Article");
        ContentTypeEntity contentType2 = ContentTypeFactory.Create("Video");
        _context.ContentTypes.Add(contentType2);
        await _context.SaveChangesAsync();

        _context.Categories.Add(CategoryFactory.Create(contentType1.Id));
        _context.Categories.Add(CategoryFactory.Create(contentType2.Id));
        await _context.SaveChangesAsync();

        // Act
        IReadOnlyList<CategoryEntity> result = await _repository.GetActiveByContentTypeAsync(contentType1.Id);

        // Assert
        result.Should().ContainSingle();
        result[0].ContentTypeId.Should().Be(contentType1.Id);
    }

    #endregion

    #region Pricing Tests

    [Fact]
    public async Task SetPricing_ThroughTheRoot_ShouldInsertTheRow()
    {
        // Arrange
        ContentTypeEntity contentType = await AddContentTypeAsync();
        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        PricingTierEntity pricingTier = PricingTierFactory.CreateDefault();

        _context.Categories.Add(category);
        _context.PricingTiers.Add(pricingTier);
        await _context.SaveChangesAsync();

        // Act
        category.SetPricing(pricingTierId: pricingTier.Id, priceUsd: 25m);
        await _context.SaveChangesAsync();

        // Assert
        CategoryPricingEntity? retrieved = await _context.CategoryPricing.FindAsync(
            category.FindPricing(pricingTier.Id)!.Id
        );
        retrieved.Should().NotBeNull();
        retrieved!.PriceUsd.Amount.Should().Be(25m);
    }

    [Fact]
    public async Task SetPricing_ThroughTheRoot_ShouldRepriceInPlace()
    {
        // Arrange
        ContentTypeEntity contentType = await AddContentTypeAsync();
        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        PricingTierEntity pricingTier = PricingTierFactory.CreateDefault();
        category.SetPricing(pricingTierId: pricingTier.Id, priceUsd: 25m);

        _context.Categories.Add(category);
        _context.PricingTiers.Add(pricingTier);
        await _context.SaveChangesAsync();

        // Act
        category.SetPricing(pricingTierId: pricingTier.Id, priceUsd: 30m);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Assert
        CategoryEntity loaded = await _repository.GetByIdOrThrowAsync(category.Id);
        loaded.Pricing.Should().ContainSingle().Which.PriceUsd.Amount.Should().Be(30m);
    }

    [Fact]
    public async Task RemovePricing_ThroughTheRoot_ShouldDeleteTheRow()
    {
        // Arrange
        ContentTypeEntity contentType = await AddContentTypeAsync();
        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        PricingTierEntity pricingTier = PricingTierFactory.CreateDefault();
        category.SetPricing(pricingTierId: pricingTier.Id, priceUsd: 25m);

        _context.Categories.Add(category);
        _context.PricingTiers.Add(pricingTier);
        await _context.SaveChangesAsync();

        // Act
        category.RemovePricing(pricingTierId: pricingTier.Id);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Assert
        CategoryEntity loaded = await _repository.GetByIdOrThrowAsync(category.Id);
        loaded.Pricing.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdOrThrowAsync_ShouldHydrateEveryPricingRow()
    {
        // Arrange
        ContentTypeEntity contentType = await AddContentTypeAsync();
        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        PricingTierEntity tier1 = PricingTierFactory.Create("tier-one");
        PricingTierEntity tier2 = PricingTierFactory.Create("tier-two");
        category.SetPricing(pricingTierId: tier1.Id, priceUsd: 10m);
        category.SetPricing(pricingTierId: tier2.Id, priceUsd: 20m);

        _context.Categories.Add(category);
        _context.PricingTiers.AddRange(tier1, tier2);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        CategoryEntity loaded = await _repository.GetByIdOrThrowAsync(category.Id);

        // Assert
        loaded.Pricing.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdsAsync_ShouldReturnEveryRequestedCategoryWithItsPricing()
    {
        // Arrange
        ContentTypeEntity contentType = await AddContentTypeAsync();
        CategoryEntity requestedOne = CategoryFactory.Create(contentType.Id);
        CategoryEntity requestedTwo = CategoryFactory.Create(contentType.Id);
        CategoryEntity unrequested = CategoryFactory.Create(contentType.Id);
        PricingTierEntity tier = PricingTierFactory.Create("tier-one");

        _context.Categories.AddRange(requestedOne, requestedTwo, unrequested);
        _context.PricingTiers.Add(tier);
        _context.CategoryPricing.AddRange(
            CategoryPricingFactory.Create(requestedOne, tier.Id),
            CategoryPricingFactory.Create(requestedTwo, tier.Id),
            CategoryPricingFactory.Create(unrequested, tier.Id)
        );
        await _context.SaveChangesAsync();

        // Act
        IReadOnlyDictionary<Guid, CategoryEntity> result = await _repository.GetByIdsAsync([
            requestedOne.Id,
            requestedTwo.Id,
        ]);

        // Assert
        result.Keys.Should().BeEquivalentTo([requestedOne.Id, requestedTwo.Id]);
        result[requestedOne.Id].Pricing.Should().ContainSingle();
    }

    [Fact]
    public async Task GetByIdsAsync_WithNoIds_ShouldReturnEmpty()
    {
        // Act
        IReadOnlyDictionary<Guid, CategoryEntity> result = await _repository.GetByIdsAsync([]);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task RemovePricing_ShouldDeleteEntityFromDatabase()
    {
        // Arrange
        ContentTypeEntity contentType = await AddContentTypeAsync();
        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        PricingTierEntity pricingTier = PricingTierFactory.CreateDefault();
        CategoryPricingEntity pricing = CategoryPricingFactory.Create(category, pricingTier.Id);

        _context.Categories.Add(category);
        _context.PricingTiers.Add(pricingTier);
        await _context.SaveChangesAsync();

        // Act
        category.RemovePricing(pricingTier.Id);
        await _context.SaveChangesAsync();

        // Assert
        CategoryPricingEntity? retrieved = await _context.CategoryPricing.FindAsync(pricing.Id);
        retrieved.Should().BeNull();
    }

    #endregion
}
