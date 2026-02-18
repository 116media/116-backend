namespace _116.Content.Application.Lookup.Constants;

/// <summary>
/// Contains route path constants for lookup-related API endpoints.
/// Provides centralized string constants for URL segments used in content type,
/// pricing tier, promotion level, and tag management routes.
/// </summary>
public static class LookupRouteConstants
{
    /// <summary>
    /// The base endpoint path for content type routes.
    /// Combined with admin prefix: /api/v1/admin/content-types.
    /// </summary>
    public const string ContentTypes = "content-types";

    /// <summary>
    /// The base endpoint path for pricing tier routes.
    /// Combined with admin prefix: /api/v1/admin/pricing-tiers.
    /// </summary>
    public const string PricingTiers = "pricing-tiers";

    /// <summary>
    /// The base endpoint path for promotion level routes.
    /// Combined with admin prefix: /api/v1/admin/promotion-levels.
    /// </summary>
    public const string PromotionLevels = "promotion-levels";

    /// <summary>
    /// The base endpoint path for tag routes.
    /// Combined with admin prefix: /api/v1/admin/tags or public prefix: /api/v1/public/tags.
    /// </summary>
    public const string Tags = "tags";

    /// <summary>
    /// Route segment for activating a lookup entity.
    /// Example: /api/v1/admin/content-types/{id}/activate.
    /// </summary>
    public const string Activate = "activate";

    /// <summary>
    /// Route segment for deactivating a lookup entity.
    /// Example: /api/v1/admin/content-types/{id}/deactivate.
    /// </summary>
    public const string Deactivate = "deactivate";
}
