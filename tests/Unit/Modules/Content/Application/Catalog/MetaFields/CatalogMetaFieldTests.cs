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
using _116.Content.Application.Catalog.UseCases.Public.Queries.GetActiveCategories;
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
    public void AdminCreateCategoryMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminCreateCategoryMetaField.AdminCreateCategory;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void AdminUpdateCategoryMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminUpdateCategoryMetaField.AdminUpdateCategory;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void AdminActivateCategoryMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminActivateCategoryMetaField.AdminActivateCategory;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void AdminDeactivateCategoryMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminDeactivateCategoryMetaField.AdminDeactivateCategory;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void AdminGetAllCategoriesMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminGetAllCategoriesMetaField.AdminGetAllCategories;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void AdminGetCategoryByIdMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminGetCategoryByIdMetaField.AdminGetCategoryById;
        metadata.Should().NotBeNull();
    }

    #endregion

    #region CategoryPricing MetaFields

    [Fact]
    public void AdminAddCategoryPricingMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminAddCategoryPricingMetaField.AdminAddCategoryPricing;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void AdminUpdateCategoryPricingMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminUpdateCategoryPricingMetaField.AdminUpdateCategoryPricing;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void AdminRemoveCategoryPricingMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminRemoveCategoryPricingMetaField.AdminRemoveCategoryPricing;
        metadata.Should().NotBeNull();
    }

    #endregion

    #region Customer MetaFields

    [Fact]
    public void AdminCreateCustomerMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminCreateCustomerMetaField.AdminCreateCustomer;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void AdminUpdateCustomerMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminUpdateCustomerMetaField.AdminUpdateCustomer;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void AdminGetAllCustomersMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminGetAllCustomersMetaField.AdminGetAllCustomers;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void AdminGetCustomerByIdMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminGetCustomerByIdMetaField.AdminGetCustomerById;
        metadata.Should().NotBeNull();
    }

    #endregion

    #region Package MetaFields

    [Fact]
    public void AdminCreatePackageMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminCreatePackageMetaField.AdminCreatePackage;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void AdminActivatePackageMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminActivatePackageMetaField.AdminActivatePackage;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void AdminDeactivatePackageMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminDeactivatePackageMetaField.AdminDeactivatePackage;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void AdminGetAllPackagesMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminGetAllPackagesMetaField.AdminGetAllPackages;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void AdminGetPackageByIdMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminGetPackageByIdMetaField.AdminGetPackageById;
        metadata.Should().NotBeNull();
    }

    #endregion

    #region PackageSlot MetaFields

    [Fact]
    public void AdminAddPackageSlotMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminAddPackageSlotMetaField.AdminAddPackageSlot;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void AdminRemovePackageSlotMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminRemovePackageSlotMetaField.AdminRemovePackageSlot;
        metadata.Should().NotBeNull();
    }

    #endregion

    #region Public Query MetaFields

    [Fact]
    public void PublicGetActiveCategoriesMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetActiveCategoriesMetaField.PublicGetActiveCategories;
        metadata.Should().NotBeNull();
    }

    #endregion
}
