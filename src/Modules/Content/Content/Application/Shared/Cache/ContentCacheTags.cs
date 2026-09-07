namespace _116.Content.Application.Shared.Cache;

/// <summary>
/// Tag names shared by the queries that cache a result and the event handlers that evict it.
/// </summary>
public static class ContentCacheTags
{
    /// <summary>
    /// Feeds ranked by article engagement or article publish state.
    /// </summary>
    public const string PopularArticles = "content:popular-articles";

    /// <summary>
    /// Feeds ranked by video engagement or video publish state.
    /// </summary>
    public const string PopularVideos = "content:popular-videos";

    /// <summary>
    /// Any projection over the tag vocabulary or its content associations.
    /// </summary>
    public const string Tags = "content:tags";

    /// <summary>
    /// Article feeds and article detail projections.
    /// </summary>
    public const string Articles = "content:articles";

    /// <summary>
    /// Video feeds and video detail projections.
    /// </summary>
    public const string Videos = "content:videos";

    /// <summary>
    /// Lyrics pages, translations and revision listings.
    /// </summary>
    public const string Lyrics = "content:lyrics";

    /// <summary>
    /// Short-video feeds and detail projections.
    /// </summary>
    public const string Shorts = "content:shorts";

    /// <summary>
    /// Artist listings, profiles, articles and releases.
    /// </summary>
    public const string Artists = "content:artists";

    /// <summary>
    /// The lookup tables: content types, pricing tiers, promotion levels, categories.
    /// </summary>
    public const string Lookups = "content:lookups";
}
