namespace _116.Content.Application.Shared.Errors.Messages;

/// <summary>
/// Provides error messages for article interaction operations (likes, bookmarks, comments).
/// </summary>
public static class ArticleInteractionErrorMessage
{
    /// <summary>
    /// Gets an error message for when a user has already liked an article.
    /// </summary>
    /// <returns>
    /// An error message indicating the article has already been liked.
    /// </returns>
    public static string AlreadyLiked()
    {
        return "You have already liked this article";
    }

    /// <summary>
    /// Gets an error message for when a like is not found for an article.
    /// </summary>
    /// <returns>
    /// An error message indicating the like was not found.
    /// </returns>
    public static string LikeNotFound()
    {
        return "Like not found for this article";
    }

    /// <summary>
    /// Gets an error message for when a user has already bookmarked an article.
    /// </summary>
    /// <returns>
    /// An error message indicating the article has already been bookmarked.
    /// </returns>
    public static string AlreadyBookmarked()
    {
        return "You have already bookmarked this article";
    }

    /// <summary>
    /// Gets an error message for when a bookmark is not found for an article.
    /// </summary>
    /// <returns>
    /// An error message indicating the bookmark was not found.
    /// </returns>
    public static string BookmarkNotFound()
    {
        return "Bookmark not found for this article";
    }

    /// <summary>
    /// Gets an error message for when a comment is not found by its identifier.
    /// </summary>
    /// <param name="commentId">The identifier of the comment that was not found.</param>
    /// <returns>
    /// A formatted error message indicating the comment was not found.
    /// </returns>
    public static string CommentNotFound(Guid commentId)
    {
        return $"Comment '{commentId}' not found";
    }

    /// <summary>
    /// Gets an error message for when a user attempts to modify a comment they do not own.
    /// </summary>
    /// <returns>
    /// An error message indicating the user can only modify their own comments.
    /// </returns>
    public static string NotCommentOwner()
    {
        return "You can only modify your own comments";
    }
}
