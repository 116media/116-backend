using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Content.Infrastructure.Repositories;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
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
            .Options;

        _context = new ContentDbContext(options);
        _repository = new CategoryRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
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
        result!.Id.Should().Be(category.Id);
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
        Guid nonExistentId = Guid.NewGuid();

        // Act
        Func<Task> act = async () => await _repository.GetByIdOrThrowAsync(nonExistentId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region GetBySlugAsync Tests

    [Fact(
        Skip = "CategoryBySlugSpecification uses EF.Functions.ILike which is not supported by InMemoryDatabase — tested in integration tests"
    )]
    public async Task GetBySlugAsync_WhenFound_ShouldReturnEntity()
    {
        // Arrange
        ContentTypeEntity contentType = await AddContentTypeAsync();
        CategoryEntity category = CategoryFactory.Create(contentType.Id, "Artist Profile", "artist-profile");
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        // Act
        CategoryEntity? result = await _repository.GetBySlugAsync("artist-profile");

        // Assert
        result.Should().NotBeNull();
        result!.Slug.Should().Be("artist-profile");
    }

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
        free.Should().HaveCount(1);
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
        result.Should().HaveCount(1);
        result[0].ContentTypeId.Should().Be(contentType1.Id);
    }

    #endregion

    #region Pricing Tests

    [Fact]
    public async Task AddPricingAsync_ShouldPersistCategoryPricingEntity()
    {
        // Arrange
        ContentTypeEntity contentType = await AddContentTypeAsync();
        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        PricingTierEntity pricingTier = PricingTierFactory.CreateDefault();

        _context.Categories.Add(category);
        _context.PricingTiers.Add(pricingTier);
        await _context.SaveChangesAsync();

        CategoryPricingEntity pricing = CategoryPricingFactory.Create(category.Id, pricingTier.Id, 25m);

        // Act
        await _repository.AddPricingAsync(pricing);
        await _context.SaveChangesAsync();

        // Assert
        CategoryPricingEntity? retrieved = await _context.CategoryPricing.FindAsync(pricing.Id);
        retrieved.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPricingAsync_WhenFound_ShouldReturnEntity()
    {
        // Arrange
        ContentTypeEntity contentType = await AddContentTypeAsync();
        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        PricingTierEntity pricingTier = PricingTierFactory.CreateDefault();
        CategoryPricingEntity pricing = CategoryPricingFactory.Create(category.Id, pricingTier.Id, 25m);

        _context.Categories.Add(category);
        _context.PricingTiers.Add(pricingTier);
        _context.CategoryPricing.Add(pricing);
        await _context.SaveChangesAsync();

        // Act
        CategoryPricingEntity? result = await _repository.GetPricingAsync(category.Id, pricingTier.Id);

        // Assert
        result.Should().NotBeNull();
        result!.CategoryId.Should().Be(category.Id);
        result.PricingTierId.Should().Be(pricingTier.Id);
    }

    [Fact]
    public async Task GetPricingAsync_WhenNotFound_ShouldReturnNull()
    {
        // Act
        CategoryPricingEntity? result = await _repository.GetPricingAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPricingByCategoryAsync_ShouldReturnAllPricingForCategory()
    {
        // Arrange
        ContentTypeEntity contentType = await AddContentTypeAsync();
        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        PricingTierEntity tier1 = PricingTierFactory.Create("tier-one");
        PricingTierEntity tier2 = PricingTierFactory.Create("tier-two");

        _context.Categories.Add(category);
        _context.PricingTiers.AddRange(tier1, tier2);
        _context.CategoryPricing.AddRange(
            CategoryPricingFactory.Create(category.Id, tier1.Id),
            CategoryPricingFactory.Create(category.Id, tier2.Id)
        );
        await _context.SaveChangesAsync();

        // Act
        IReadOnlyList<CategoryPricingEntity> result = await _repository.GetPricingByCategoryAsync(category.Id);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task RemovePricing_ShouldDeleteEntityFromDatabase()
    {
        // Arrange
        ContentTypeEntity contentType = await AddContentTypeAsync();
        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        PricingTierEntity pricingTier = PricingTierFactory.CreateDefault();
        CategoryPricingEntity pricing = CategoryPricingFactory.Create(category.Id, pricingTier.Id);

        _context.Categories.Add(category);
        _context.PricingTiers.Add(pricingTier);
        _context.CategoryPricing.Add(pricing);
        await _context.SaveChangesAsync();

        // Act
        _repository.RemovePricing(pricing);
        await _context.SaveChangesAsync();

        // Assert
        CategoryPricingEntity? retrieved = await _context.CategoryPricing.FindAsync(pricing.Id);
        retrieved.Should().BeNull();
    }

    #endregion
}
