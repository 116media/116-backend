using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Infrastructure.Persistence;

/// <summary>
/// Unit tests for <see cref="ContentDbContext"/>.
/// </summary>
public class ContentDbContextTests
{
    private DbContextOptions<ContentDbContext> CreateOptions()
    {
        return new DbContextOptionsBuilder<ContentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    #region DbSet Tests

    [Fact]
    public void ContentTypes_ShouldReturnDbSet()
    {
        // Arrange
        DbContextOptions<ContentDbContext> options = CreateOptions();
        using var context = new ContentDbContext(options);

        // Act
        DbSet<ContentTypeEntity> result = context.ContentTypes;

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void PricingTiers_ShouldReturnDbSet()
    {
        // Arrange
        DbContextOptions<ContentDbContext> options = CreateOptions();
        using var context = new ContentDbContext(options);

        // Act
        DbSet<PricingTierEntity> result = context.PricingTiers;

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void PromotionLevels_ShouldReturnDbSet()
    {
        // Arrange
        DbContextOptions<ContentDbContext> options = CreateOptions();
        using var context = new ContentDbContext(options);

        // Act
        DbSet<PromotionLevelEntity> result = context.PromotionLevels;

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void Tags_ShouldReturnDbSet()
    {
        // Arrange
        DbContextOptions<ContentDbContext> options = CreateOptions();
        using var context = new ContentDbContext(options);

        // Act
        DbSet<TagEntity> result = context.Tags;

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void Categories_ShouldReturnDbSet()
    {
        // Arrange
        DbContextOptions<ContentDbContext> options = CreateOptions();
        using var context = new ContentDbContext(options);

        // Act
        DbSet<CategoryEntity> result = context.Categories;

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void CategoryPricing_ShouldReturnDbSet()
    {
        // Arrange
        DbContextOptions<ContentDbContext> options = CreateOptions();
        using var context = new ContentDbContext(options);

        // Act
        DbSet<CategoryPricingEntity> result = context.CategoryPricing;

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void Customers_ShouldReturnDbSet()
    {
        // Arrange
        DbContextOptions<ContentDbContext> options = CreateOptions();
        using var context = new ContentDbContext(options);

        // Act
        DbSet<CustomerEntity> result = context.Customers;

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void Packages_ShouldReturnDbSet()
    {
        // Arrange
        DbContextOptions<ContentDbContext> options = CreateOptions();
        using var context = new ContentDbContext(options);

        // Act
        DbSet<PackageEntity> result = context.Packages;

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void PackageSlots_ShouldReturnDbSet()
    {
        // Arrange
        DbContextOptions<ContentDbContext> options = CreateOptions();
        using var context = new ContentDbContext(options);

        // Act
        DbSet<PackageSlotEntity> result = context.PackageSlots;

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Model Configuration Tests

    [Fact]
    public void OnModelCreating_ShouldApplyConfigurationsFromAssembly()
    {
        // Arrange
        DbContextOptions<ContentDbContext> options = CreateOptions();
        using var context = new ContentDbContext(options);

        // Act
        IModel model = context.Model;

        // Assert
        IEntityType? categoryEntityType = model.FindEntityType(typeof(CategoryEntity));
        categoryEntityType.Should().NotBeNull();
    }

    [Fact]
    public void Context_ShouldHaveAllEntityTypesConfigured()
    {
        // Arrange
        DbContextOptions<ContentDbContext> options = CreateOptions();
        using var context = new ContentDbContext(options);

        // Act
        IModel model = context.Model;

        // Assert
        model.FindEntityType(typeof(ContentTypeEntity)).Should().NotBeNull();
        model.FindEntityType(typeof(PricingTierEntity)).Should().NotBeNull();
        model.FindEntityType(typeof(PromotionLevelEntity)).Should().NotBeNull();
        model.FindEntityType(typeof(TagEntity)).Should().NotBeNull();
        model.FindEntityType(typeof(CategoryEntity)).Should().NotBeNull();
        model.FindEntityType(typeof(CategoryPricingEntity)).Should().NotBeNull();
        model.FindEntityType(typeof(CustomerEntity)).Should().NotBeNull();
        model.FindEntityType(typeof(PackageEntity)).Should().NotBeNull();
        model.FindEntityType(typeof(PackageSlotEntity)).Should().NotBeNull();
    }

    [Fact]
    public async Task Context_ShouldSaveAndRetrieveContentTypeEntity()
    {
        // Arrange
        DbContextOptions<ContentDbContext> options = CreateOptions();
        await using var context = new ContentDbContext(options);

        var contentType = ContentTypeEntity.Create(id: Guid.NewGuid(), name: "Article");

        // Act
        await context.ContentTypes.AddAsync(contentType);
        await context.SaveChangesAsync();

        // Assert
        ContentTypeEntity? retrieved = await context.ContentTypes.FindAsync(contentType.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Article");
    }

    #endregion
}
