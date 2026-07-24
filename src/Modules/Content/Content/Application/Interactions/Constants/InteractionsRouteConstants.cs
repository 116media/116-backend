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
    /// The base endpoint path for lyrics interaction routes.
    /// </summary>
    public const string Lyrics = "lyrics";

    /// <summary>
    /// The base endpoint path for playlist routes.
    /// </summary>
    public const string Playlists = "playlists";

    /// <summary>
    /// Route segment for like sub-resources.
    /// </summary>
    public const string Likes = "likes";

    /// <summary>
    /// Route segment for the current user's liked-content collection.
    /// </summary>
    public const string Liked = "liked";

    /// <summary>
    /// Route segment for bookmark sub-resources.
    /// </summary>
    public const string Bookmarks = "bookmarks";

    /// <summary>
    /// Route segment for the current user's bookmarked-content collection.
    /// </summary>
    public const string Bookmarked = "bookmarked";

    /// <summary>
    /// Route segment for share sub-resources.
    /// </summary>
    public const string Shares = "shares";

    /// <summary>
    /// Route segment for the current user's grouped share collection.
    /// </summary>
    public const string Shared = "shared";

    /// <summary>
    /// Route segment for comment sub-resources.
    /// </summary>
    public const string Comments = "comments";

    /// <summary>
    /// Route segment for the current user's commented-content collection.
    /// </summary>
    public const string Commented = "commented";

    /// <summary>
    /// Route segment for resources belonging to the current user.
    /// </summary>
    public const string Me = "me";

    /// <summary>
    /// Route segment for reply sub-resources within a comment.
    /// </summary>
    public const string Replies = "replies";

    /// <summary>
    /// Route segment for rating sub-resources.
    /// </summary>
    public const string Ratings = "ratings";

    /// <summary>
    /// Route segment for the current user's rated-content collection.
    /// </summary>
    public const string Rated = "rated";

    /// <summary>
    /// Route segment for video sub-resources within playlists.
    /// </summary>
    public const string PlaylistVideos = "videos";

    /// <summary>
    /// Route segment for view sub-resources (view's count tracking).
    /// </summary>
    public const string Views = "views";
}
