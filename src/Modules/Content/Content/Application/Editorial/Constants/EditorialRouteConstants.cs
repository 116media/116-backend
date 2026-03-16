namespace _116.Content.Application.Editorial.Constants;

/// <summary>
/// URL path segments for Editorial sub-module routes.
/// Provides centralized string constants for URL segments used in article, video,
/// short video, and lyrics management routes.
/// </summary>
public static class EditorialRouteConstants
{
    /// <summary>
    /// The base endpoint path for article routes.
    /// Combined with admin prefix: /api/v1/admin/articles.
    /// </summary>
    public const string Articles = "articles";

    /// <summary>
    /// The base endpoint path for video routes.
    /// Combined with admin prefix: /api/v1/admin/videos.
    /// </summary>
    public const string Videos = "videos";

    /// <summary>
    /// The base endpoint path for short video routes.
    /// Combined with admin prefix: /api/v1/admin/shorts.
    /// </summary>
    public const string Shorts = "shorts";

    /// <summary>
    /// The base endpoint path for lyrics routes.
    /// Combined with admin prefix: /api/v1/admin/lyrics or public prefix: /api/v1/public/lyrics.
    /// </summary>
    public const string Lyrics = "lyrics";

    /// <summary>
    /// Route segment for submitting an editorial entity for review or payment.
    /// Example: /api/v1/admin/articles/{id}/submit.
    /// </summary>
    public const string Submit = "submit";

    /// <summary>
    /// Route segment for approving an editorial entity.
    /// Example: /api/v1/admin/articles/{id}/approve.
    /// </summary>
    public const string Approve = "approve";

    /// <summary>
    /// Route segment for publishing an editorial entity.
    /// Example: /api/v1/admin/articles/{id}/publish.
    /// </summary>
    public const string Publish = "publish";

    /// <summary>
    /// Route segment for rejecting an editorial entity.
    /// Example: /api/v1/admin/articles/{id}/reject.
    /// </summary>
    public const string Reject = "reject";

    /// <summary>
    /// Route segment for archiving an editorial entity.
    /// Example: /api/v1/admin/articles/{id}/archive.
    /// </summary>
    public const string Archive = "archive";

    /// <summary>
    /// Route segment for article image upload and listing sub-resource.
    /// Example: /api/v1/admin/articles/{id}/images.
    /// </summary>
    public const string Images = "images";

    /// <summary>
    /// Route segment for updating tag associations on an editorial entity.
    /// Example: /api/v1/admin/articles/{id}/tags.
    /// </summary>
    public const string Tags = "tags";

    /// <summary>
    /// Route segment for SEO metadata operations on an editorial entity.
    /// Example: /api/v1/admin/articles/{id}/seo.
    /// </summary>
    public const string Seo = "seo";

    /// <summary>
    /// Route segment for attaching a YouTube video identifier to a video.
    /// Example: /api/v1/admin/videos/{id}/youtube.
    /// </summary>
    public const string Youtube = "youtube";

    /// <summary>
    /// Route segment for thumbnail image upload on a video or short video.
    /// Example: /api/v1/admin/videos/{id}/thumbnail.
    /// </summary>
    public const string Thumbnail = "thumbnail";

    /// <summary>
    /// Route segment for scheduling a video shoot.
    /// Example: /api/v1/admin/videos/{id}/shoot.
    /// </summary>
    public const string Shoot = "shoot";

    /// <summary>
    /// Route segment for activating an editorial entity.
    /// Example: /api/v1/admin/shorts/{id}/activate.
    /// </summary>
    public const string Activate = "activate";

    /// <summary>
    /// Route segment for deactivating an editorial entity.
    /// Example: /api/v1/admin/shorts/{id}/deactivate.
    /// </summary>
    public const string Deactivate = "deactivate";

    /// <summary>
    /// Route segment for retrieving featured editorial entities.
    /// Example: /api/v1/public/articles/featured.
    /// </summary>
    public const string Featured = "featured";
}
