using _116.Content.Application.Catalog.Specifications;
using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Factories.Content;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.Specifications;

/// <summary>
/// Unit tests for catalog specification classes.
/// Note: Specifications using EF.Functions.ILike require a real PostgreSQL provider —
/// those are covered in integration tests.
/// </summary>
public class CatalogSpecificationsTests
{
    private static readonly Guid ContentTypeId = Guid.NewGuid();

    #region InactivePackageSpecification

    [Fact]
    public void InactivePackageSpecification_WithInactivePackage_ShouldReturnTrue()
    {
        // Arrange
        PackageEntity package = PackageFactory.CreateInactive();
        var spec = new InactivePackageSpecification();
        Func<PackageEntity, bool> predicate = spec.ToExpression().Compile();

        // Act & Assert
        predicate(package).Should().BeTrue();
    }

    [Fact]
    public void InactivePackageSpecification_WithActivePackage_ShouldReturnFalse()
    {
        // Arrange
        PackageEntity package = PackageFactory.Create();
        var spec = new InactivePackageSpecification();
        Func<PackageEntity, bool> predicate = spec.ToExpression().Compile();

        // Act & Assert
        predicate(package).Should().BeFalse();
    }

    #endregion

    #region CategoryByIdSpecification

    [Fact]
    public void CategoryByIdSpecification_WithMatchingId_ShouldReturnTrue()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(ContentTypeId);
        var spec = new CategoryByIdSpecification(category.Id);
        Func<CategoryEntity, bool> predicate = spec.ToExpression().Compile();

        // Act & Assert
        predicate(category).Should().BeTrue();
    }

    [Fact]
    public void CategoryByIdSpecification_WithDifferentId_ShouldReturnFalse()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(ContentTypeId);
        var spec = new CategoryByIdSpecification(Guid.NewGuid());
        Func<CategoryEntity, bool> predicate = spec.ToExpression().Compile();

        // Act & Assert
        predicate(category).Should().BeFalse();
    }

    #endregion

    #region ActiveCategorySpecification

    [Fact]
    public void ActiveCategorySpecification_WithActiveCategory_ShouldReturnTrue()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(ContentTypeId);
        var spec = new ActiveCategorySpecification();
        Func<CategoryEntity, bool> predicate = spec.ToExpression().Compile();

        // Act & Assert
        predicate(category).Should().BeTrue();
    }

    [Fact]
    public void ActiveCategorySpecification_WithInactiveCategory_ShouldReturnFalse()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.CreateInactive(ContentTypeId);
        var spec = new ActiveCategorySpecification();
        Func<CategoryEntity, bool> predicate = spec.ToExpression().Compile();

        // Act & Assert
        predicate(category).Should().BeFalse();
    }

    #endregion

    #region FreeCategorySpecification

    [Fact]
    public void FreeCategorySpecification_WithFreeCategory_ShouldReturnTrue()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.CreateFree(ContentTypeId);
        var spec = new FreeCategorySpecification();
        Func<CategoryEntity, bool> predicate = spec.ToExpression().Compile();

        // Act & Assert
        predicate(category).Should().BeTrue();
    }

    [Fact]
    public void FreeCategorySpecification_WithPaidCategory_ShouldReturnFalse()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.CreatePaid(ContentTypeId);
        var spec = new FreeCategorySpecification();
        Func<CategoryEntity, bool> predicate = spec.ToExpression().Compile();

        // Act & Assert
        predicate(category).Should().BeFalse();
    }

    #endregion

    #region PaidCategorySpecification

    [Fact]
    public void PaidCategorySpecification_WithPaidCategory_ShouldReturnTrue()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.CreatePaid(ContentTypeId);
        var spec = new PaidCategorySpecification();
        Func<CategoryEntity, bool> predicate = spec.ToExpression().Compile();

        // Act & Assert
        predicate(category).Should().BeTrue();
    }

    [Fact]
    public void PaidCategorySpecification_WithFreeCategory_ShouldReturnFalse()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.CreateFree(ContentTypeId);
        var spec = new PaidCategorySpecification();
        Func<CategoryEntity, bool> predicate = spec.ToExpression().Compile();

        // Act & Assert
        predicate(category).Should().BeFalse();
    }

    #endregion

    #region CategoryByContentTypeSpecification

    [Fact]
    public void CategoryByContentTypeSpecification_WithMatchingContentType_ShouldReturnTrue()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(ContentTypeId);
        var spec = new CategoryByContentTypeSpecification(ContentTypeId);
        Func<CategoryEntity, bool> predicate = spec.ToExpression().Compile();

        // Act & Assert
        predicate(category).Should().BeTrue();
    }

    [Fact]
    public void CategoryByContentTypeSpecification_WithDifferentContentType_ShouldReturnFalse()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(ContentTypeId);
        var spec = new CategoryByContentTypeSpecification(Guid.NewGuid());
        Func<CategoryEntity, bool> predicate = spec.ToExpression().Compile();

        // Act & Assert
        predicate(category).Should().BeFalse();
    }

    #endregion

    #region CategoryPricingByCategorySpecification

    [Fact]
    public void CategoryPricingByCategorySpecification_WithMatchingCategoryId_ShouldReturnTrue()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        CategoryPricingEntity pricing = CategoryPricingFactory.Create(categoryId, Guid.NewGuid());
        var spec = new CategoryPricingByCategorySpecification(categoryId);
        Func<CategoryPricingEntity, bool> predicate = spec.ToExpression().Compile();

        // Act & Assert
        predicate(pricing).Should().BeTrue();
    }

    [Fact]
    public void CategoryPricingByCategorySpecification_WithDifferentCategoryId_ShouldReturnFalse()
    {
        // Arrange
        CategoryPricingEntity pricing = CategoryPricingFactory.Create(Guid.NewGuid(), Guid.NewGuid());
        var spec = new CategoryPricingByCategorySpecification(Guid.NewGuid());
        Func<CategoryPricingEntity, bool> predicate = spec.ToExpression().Compile();

        // Act & Assert
        predicate(pricing).Should().BeFalse();
    }

    #endregion

    #region CategoryPricingByIdsSpecification

    [Fact]
    public void CategoryPricingByIdsSpecification_WithMatchingIds_ShouldReturnTrue()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var tierId = Guid.NewGuid();
        CategoryPricingEntity pricing = CategoryPricingFactory.Create(categoryId, tierId);
        var spec = new CategoryPricingByIdsSpecification(categoryId, tierId);
        Func<CategoryPricingEntity, bool> predicate = spec.ToExpression().Compile();

        // Act & Assert
        predicate(pricing).Should().BeTrue();
    }

    [Fact]
    public void CategoryPricingByIdsSpecification_WithDifferentTierId_ShouldReturnFalse()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        CategoryPricingEntity pricing = CategoryPricingFactory.Create(categoryId, Guid.NewGuid());
        var spec = new CategoryPricingByIdsSpecification(categoryId, Guid.NewGuid());
        Func<CategoryPricingEntity, bool> predicate = spec.ToExpression().Compile();

        // Act & Assert
        predicate(pricing).Should().BeFalse();
    }

    #endregion
}
