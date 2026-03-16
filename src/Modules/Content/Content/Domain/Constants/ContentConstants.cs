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

    /// <summary>
    /// Maximum allowed length for an article or video title.
    /// </summary>
    public const int MaxTitleLength = 100;

    /// <summary>
    /// Maximum allowed length for an article or video URL slug.
    /// </summary>
    public const int MaxSlugLength = 220;

    /// <summary>
    /// Maximum allowed length for the article headline (short teaser/aperçu).
    /// Minimum length of 100 is enforced at the application layer only
    /// (drafts start with an empty string and the min is checked on <c>PUT</c>).
    /// </summary>
    public const int MaxHeadlineLength = 300;

    /// <summary>
    /// Minimum allowed length for the article headline, enforced on PUT/update only.
    /// </summary>
    public const int MinHeadlineLength = 100;

    /// <summary>
    /// Maximum allowed length for an article or video rejection reason.
    /// </summary>
    public const int MaxRejectionReasonLength = 500;

    /// <summary>
    /// Minimum allowed length for the SEO meta title.
    /// </summary>
    public const int MinMetaTitleLength = 10;

    /// <summary>
    /// Maximum allowed length for the SEO meta title (Google truncates at ~60–70 chars).
    /// </summary>
    public const int MaxMetaTitleLength = 70;

    /// <summary>
    /// Minimum allowed length for an SEO meta description.
    /// </summary>
    public const int MinMetaDescriptionLength = 50;

    /// <summary>
    /// Maximum allowed length for an SEO meta description (Google truncates at ~155–160 chars).
    /// </summary>
    public const int MaxMetaDescriptionLength = 160;

    /// <summary>
    /// Maximum allowed length for a cover image URL stored on an article.
    /// </summary>
    public const int MaxCoverImageUrlLength = 500;

    /// <summary>
    /// Maximum allowed length for a YouTube video ID (e.g., "dQw4w9WgXcQ" is 11 chars;
    /// padded to 20 to accommodate future ID format changes).
    /// </summary>
    public const int MaxYoutubeVideoIdLength = 20;

    /// <summary>
    /// Maximum allowed length for a video thumbnail URL.
    /// </summary>
    public const int MaxThumbnailUrlLength = 500;

    /// <summary>
    /// Maximum allowed length for a short video title.
    /// </summary>
    public const int MaxShortVideoTitleLength = 200;

    /// <summary>
    /// Maximum allowed length for a short video URL (public CDN URL).
    /// </summary>
    public const int MaxShortVideoUrlLength = 500;

    /// <summary>
    /// Maximum allowed length for a song title on a lyrics page.
    /// </summary>
    public const int MaxSongTitleLength = 200;

    /// <summary>
    /// Maximum allowed length for an artist name on a lyrics page.
    /// </summary>
    public const int MaxArtistNameLength = 100;

    /// <summary>
    /// Maximum allowed length for the lyrics language code (ISO 639-1, e.g. "fr", "ln", "en").
    /// Padded to 5 to accommodate BCP-47 subtags (e.g., "fr-CD").
    /// </summary>
    public const int MaxLyricsLanguageLength = 5;

    /// <summary>
    /// Default language for lyrics when none is specified.
    /// </summary>
    public const string DefaultLyricsLanguage = "fr";

    /// <summary>
    /// Maximum allowed length for SEO meta keywords on a lyrics page.
    /// </summary>
    public const int MaxMetaKeywordsLength = 300;
}
