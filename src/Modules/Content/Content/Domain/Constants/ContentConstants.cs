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

    /// <summary>
    /// Maximum allowed length for a category name field (e.g., "Artist Profile", "116 Le Focus").
    /// Used in entity validation and Entity Framework property configuration.
    /// </summary>
    public const int MaxCategoryNameLength = 60;

    /// <summary>
    /// Maximum allowed length for a category slug field (e.g., "artist-profile", "116-le-focus").
    /// Used in entity validation and Entity Framework property configuration.
    /// </summary>
    public const int MaxCategorySlugLength = 80;

    /// <summary>
    /// Maximum allowed length for a category description field.
    /// Used in entity validation and Entity Framework property configuration.
    /// </summary>
    public const int MaxCategoryDescriptionLength = 300;

    /// <summary>
    /// Maximum allowed length for a customer full name field.
    /// Used in entity validation and Entity Framework property configuration.
    /// </summary>
    public const int MaxCustomerFullNameLength = 100;

    /// <summary>
    /// Maximum allowed length for a customer email field.
    /// Used in entity validation and Entity Framework property configuration.
    /// </summary>
    public const int MaxCustomerEmailLength = 200;

    /// <summary>
    /// Maximum allowed length for a customer phone field.
    /// Used in entity validation and Entity Framework property configuration.
    /// </summary>
    public const int MaxCustomerPhoneLength = 30;

    /// <summary>
    /// Maximum allowed length for a customer company field.
    /// Used in entity validation and Entity Framework property configuration.
    /// </summary>
    public const int MaxCustomerCompanyLength = 100;

    /// <summary>
    /// Maximum allowed length for a customer notes field.
    /// Used in entity validation and Entity Framework property configuration.
    /// </summary>
    public const int MaxCustomerNotesLength = 500;

    /// <summary>
    /// Maximum allowed length for a package name field.
    /// Used in entity validation and Entity Framework property configuration.
    /// </summary>
    public const int MaxPackageNameLength = 100;

    /// <summary>
    /// Maximum allowed length for a package description field.
    /// Used in entity validation and Entity Framework property configuration.
    /// </summary>
    public const int MaxPackageDescriptionLength = 500;
}
