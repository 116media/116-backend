namespace _116.Content.Domain.Constants;

/// <summary>
/// Contains constant values for the Content module.
/// Provides centralized string constants for module identification, route prefixes,
/// database schema naming, and entity field length constraints to ensure consistency
/// across the application.
/// </summary>
public static class ContentConstants
{
    /// <summary>
    /// Database schema name for content-related tables.
    /// Used in Entity Framework configurations to organize content tables under the "content" schema.
    /// </summary>
    public const string SchemaName = "content";

    /// <summary>
    /// Identifies the Content module within the application.
    /// Used for module registration and configuration.
    /// </summary>
    public const string ModuleName = "Content";

    /// <summary>
    /// Route prefix for administrative content endpoints.
    /// Used to construct admin-specific API routes (e.g., /api/v1/admin/content-types).
    /// </summary>
    public const string Admin = "admin";

    /// <summary>
    /// Route prefix for public content endpoints.
    /// Used to construct publicly accessible API routes (e.g., /api/v1/public/tags).
    /// </summary>
    public const string Public = "public";

    /// <summary>
    /// Maximum allowed length for a content type name field (e.g., "Article", "Video").
    /// Used in entity validation and Entity Framework property configuration.
    /// </summary>
    public const int MaxContentTypeNameLength = 30;

    /// <summary>
    /// Maximum allowed length for a pricing tier name field (e.g., "base_upload", "social_boost").
    /// Used in entity validation and Entity Framework property configuration.
    /// </summary>
    public const int MaxPricingTierNameLength = 40;

    /// <summary>
    /// Maximum allowed length for a pricing tier description field.
    /// Used in entity validation and Entity Framework property configuration.
    /// </summary>
    public const int MaxPricingTierDescriptionLength = 200;

    /// <summary>
    /// Maximum allowed length for a promotion level name field (e.g., "Featured — 7 days").
    /// Used in entity validation and Entity Framework property configuration.
    /// </summary>
    public const int MaxPromotionLevelNameLength = 40;

    /// <summary>
    /// Maximum allowed length for a tag name field (e.g., "Fally Ipupa", "Kinshasa").
    /// Used in entity validation and Entity Framework property configuration.
    /// </summary>
    public const int MaxTagNameLength = 50;

    /// <summary>
    /// Maximum allowed length for a tag slug field (e.g., "fally-ipupa", "kinshasa").
    /// Used in entity validation and Entity Framework property configuration.
    /// </summary>
    public const int MaxTagSlugLength = 60;
}
