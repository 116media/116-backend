namespace _116.Content.Application.Catalog.Constants;

/// <summary>
/// Contains route path constants for catalog-related API endpoints.
/// Provides centralized string constants for URL segments used in category,
/// customer, and package management routes.
/// </summary>
public static class CatalogRouteConstants
{
    /// <summary>
    /// The base endpoint path for category routes.
    /// Combined with admin prefix: /api/v1/admin/categories.
    /// </summary>
    public const string Categories = "categories";

    /// <summary>
    /// The base endpoint path for customer routes.
    /// Combined with admin prefix: /api/v1/admin/customers.
    /// </summary>
    public const string Customers = "customers";

    /// <summary>
    /// The base endpoint path for package routes.
    /// Combined with admin prefix: /api/v1/admin/packages.
    /// </summary>
    public const string Packages = "packages";

    /// <summary>
    /// Route segment for category pricing sub-resource.
    /// Example: /api/v1/admin/categories/{id}/pricing.
    /// </summary>
    public const string Pricing = "pricing";

    /// <summary>
    /// Route segment for package slots sub-resource.
    /// Example: /api/v1/admin/packages/{id}/slots.
    /// </summary>
    public const string Slots = "slots";

    /// <summary>
    /// Route segment for activating a catalog entity.
    /// Example: /api/v1/admin/categories/{id}/activate.
    /// </summary>
    public const string Activate = "activate";

    /// <summary>
    /// Route segment for deactivating a catalog entity.
    /// Example: /api/v1/admin/categories/{id}/deactivate.
    /// </summary>
    public const string Deactivate = "deactivate";
}
