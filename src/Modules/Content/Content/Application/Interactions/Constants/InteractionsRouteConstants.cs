namespace _116.Content.Application.Interactions.Constants;

/// <summary>
/// URL path segments for Interactions sub-module routes.
/// Provides centralized string constants for URL segments used in like, bookmark,
/// share, comment, rating, playlist, and view management routes.
/// </summary>
public static class InteractionsRouteConstants
{
    /// <summary>
    /// The base endpoint path for article interaction routes.
    /// </summary>
    public const string Articles = "articles";

    /// <summary>
    /// The base endpoint path for video interaction routes.
    /// </summary>
    public const string Videos = "videos";

    /// <summary>
    /// The base endpoint path for short video interaction routes.
    /// </summary>
    public const string Shorts = "shorts";

    /// <summary>
    /// The base endpoint path for playlist routes.
    /// </summary>
    public const string Playlists = "playlists";

    /// <summary>
    /// Route segment for like sub-resources.
    /// </summary>
    public const string Likes = "likes";

    /// <summary>
    /// Route segment for bookmark sub-resources.
    /// </summary>
    public const string Bookmarks = "bookmarks";

    /// <summary>
    /// Route segment for share sub-resources.
    /// </summary>
    public const string Shares = "shares";

    /// <summary>
    /// Route segment for comment sub-resources.
    /// </summary>
    public const string Comments = "comments";

    /// <summary>
    /// Route segment for rating sub-resources.
    /// </summary>
    public const string Ratings = "ratings";

    /// <summary>
    /// Route segment for video sub-resources within playlists.
    /// </summary>
    public const string PlaylistVideos = "videos";

    /// <summary>
    /// Route segment for view sub-resources (view's count tracking).
    /// </summary>
    public const string Views = "views";
}
