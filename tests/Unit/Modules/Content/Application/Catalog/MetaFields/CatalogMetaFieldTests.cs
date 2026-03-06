using _116.Content.Application.Catalog.UseCases.Admin.Commands.ActivateCategory;
using _116.Content.Application.Catalog.UseCases.Admin.Commands.ActivatePackage;
using _116.Content.Application.Catalog.UseCases.Admin.Commands.AddCategoryPricing;
using _116.Content.Application.Catalog.UseCases.Admin.Commands.AddPackageSlot;
using _116.Content.Application.Catalog.UseCases.Admin.Commands.CreateCategory;
using _116.Content.Application.Catalog.UseCases.Admin.Commands.CreateCustomer;
using _116.Content.Application.Catalog.UseCases.Admin.Commands.CreatePackage;
using _116.Content.Application.Catalog.UseCases.Admin.Commands.DeactivateCategory;
using _116.Content.Application.Catalog.UseCases.Admin.Commands.DeactivatePackage;
using _116.Content.Application.Catalog.UseCases.Admin.Commands.RemoveCategoryPricing;
using _116.Content.Application.Catalog.UseCases.Admin.Commands.RemovePackageSlot;
using _116.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCategory;
using _116.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCategoryPricing;
using _116.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCustomer;
using _116.Content.Application.Catalog.UseCases.Admin.Queries.GetAllCategories;
using _116.Content.Application.Catalog.UseCases.Admin.Queries.GetAllCustomers;
using _116.Content.Application.Catalog.UseCases.Admin.Queries.GetAllPackages;
using _116.Content.Application.Catalog.UseCases.Admin.Queries.GetCategoryById;
using _116.Content.Application.Catalog.UseCases.Admin.Queries.GetCustomerById;
using _116.Content.Application.Catalog.UseCases.Admin.Queries.GetPackageById;
using _116.Content.Application.Catalog.UseCases.Public.Queries.GetPublicCategories;
using _116.Shared.Application.Metadata;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.MetaFields;

/// <summary>
/// Tests that all Catalog MetaField static fields are correctly initialized.
/// Accessing each static readonly field triggers its initializer, ensuring full coverage.
/// </summary>
public class CatalogMetaFieldTests
{
    #region Category MetaFields

    [Fact]
    public void CreateCategoryMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = CreateCategoryMetaField.CreateCategory;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void UpdateCategoryMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = UpdateCategoryMetaField.UpdateCategory;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void ActivateCategoryMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = ActivateCategoryMetaField.ActivateCategory;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void DeactivateCategoryMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = DeactivateCategoryMetaField.DeactivateCategory;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void GetAllCategoriesMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = GetAllCategoriesMetaField.GetAllCategories;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void GetCategoryByIdMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = GetCategoryByIdMetaField.GetCategoryById;
        metadata.Should().NotBeNull();
    }

    #endregion

    #region CategoryPricing MetaFields

    [Fact]
    public void AddCategoryPricingMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AddCategoryPricingMetaField.AddCategoryPricing;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void UpdateCategoryPricingMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = UpdateCategoryPricingMetaField.UpdateCategoryPricing;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void RemoveCategoryPricingMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = RemoveCategoryPricingMetaField.RemoveCategoryPricing;
        metadata.Should().NotBeNull();
    }

    #endregion

    #region Customer MetaFields

    [Fact]
    public void CreateCustomerMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = CreateCustomerMetaField.CreateCustomer;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void UpdateCustomerMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = UpdateCustomerMetaField.UpdateCustomer;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void GetAllCustomersMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = GetAllCustomersMetaField.GetAllCustomers;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void GetCustomerByIdMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = GetCustomerByIdMetaField.GetCustomerById;
        metadata.Should().NotBeNull();
    }

    #endregion

    #region Package MetaFields

    [Fact]
    public void CreatePackageMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = CreatePackageMetaField.CreatePackage;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void ActivatePackageMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = ActivatePackageMetaField.ActivatePackage;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void DeactivatePackageMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = DeactivatePackageMetaField.DeactivatePackage;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void GetAllPackagesMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = GetAllPackagesMetaField.GetAllPackages;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void GetPackageByIdMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = GetPackageByIdMetaField.GetPackageById;
        metadata.Should().NotBeNull();
    }

    #endregion

    #region PackageSlot MetaFields

    [Fact]
    public void AddPackageSlotMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AddPackageSlotMetaField.AddPackageSlot;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void RemovePackageSlotMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = RemovePackageSlotMetaField.RemovePackageSlot;
        metadata.Should().NotBeNull();
    }

    #endregion

    #region Public Query MetaFields

    [Fact]
    public void GetPublicCategoriesMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = GetPublicCategoriesMetaField.GetPublicCategories;
        metadata.Should().NotBeNull();
    }

    #endregion
}
