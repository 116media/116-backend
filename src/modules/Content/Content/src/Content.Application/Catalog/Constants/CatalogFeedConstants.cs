namespace _116.Content.Application.Catalog.Constants;

/// <summary>
/// Constants governing how categories are curated into the content feed.
/// </summary>
public static class CatalogFeedConstants
{
    /// <summary>
    /// Maximum number of categories that may be pinned to the feed at once, per content type.
    /// Pinning a category beyond this limit unpins the oldest pinned category of the same
    /// content type (FIFO).
    /// </summary>
    public const int MaxPinnedCategoriesPerContentType = 5;
}
