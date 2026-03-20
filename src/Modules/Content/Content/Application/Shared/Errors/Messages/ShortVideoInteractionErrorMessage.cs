namespace _116.Content.Application.Shared.Errors.Messages;

/// <summary>
/// Provides error messages for short video interaction operations (likes, bookmarks).
/// </summary>
public static class ShortVideoInteractionErrorMessage
{
    /// <summary>
    /// Gets an error message for when a user has already liked a short video.
    /// </summary>
    /// <returns>
    /// An error message indicating the short video has already been liked.
    /// </returns>
    public static string AlreadyLiked()
    {
        return "You have already liked this short video";
    }

    /// <summary>
    /// Gets an error message for when a like is not found for a short video.
    /// </summary>
    /// <returns>
    /// An error message indicating the like was not found.
    /// </returns>
    public static string LikeNotFound()
    {
        return "Like not found for this short video";
    }

    /// <summary>
    /// Gets an error message for when a user has already bookmarked a short video.
    /// </summary>
    /// <returns>
    /// An error message indicating the short video has already been bookmarked.
    /// </returns>
    public static string AlreadyBookmarked()
    {
        return "You have already bookmarked this short video";
    }

    /// <summary>
    /// Gets an error message for when a bookmark is not found for a short video.
    /// </summary>
    /// <returns>
    /// An error message indicating the bookmark was not found.
    /// </returns>
    public static string BookmarkNotFound()
    {
        return "Bookmark not found for this short video";
    }
}
