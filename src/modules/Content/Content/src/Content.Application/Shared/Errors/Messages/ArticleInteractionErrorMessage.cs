using Microsoft.Extensions.Localization;

namespace _116.Content.Application.Shared.Errors.Messages;

/// <summary>
/// Provides error messages for article interaction operations (likes, bookmarks, comments).
/// </summary>
public class ArticleInteractionErrorMessage(IStringLocalizer<ArticleInteractionErrorMessage> localizer)
{
    /// <summary>
    /// Gets an error message for when a user has already liked an article.
    /// </summary>
    /// <returns>
    /// An error message indicating the article has already been liked.
    /// </returns>
    public string AlreadyLiked()
    {
        return localizer["AlreadyLiked"];
    }

    /// <summary>
    /// Gets an error message for when a like is not found for an article.
    /// </summary>
    /// <returns>
    /// An error message indicating the like was not found.
    /// </returns>
    public string LikeNotFound()
    {
        return localizer["LikeNotFound"];
    }

    /// <summary>
    /// Gets an error message for when a user has already bookmarked an article.
    /// </summary>
    /// <returns>
    /// An error message indicating the article has already been bookmarked.
    /// </returns>
    public string AlreadyBookmarked()
    {
        return localizer["AlreadyBookmarked"];
    }

    /// <summary>
    /// Gets an error message for when a bookmark is not found for an article.
    /// </summary>
    /// <returns>
    /// An error message indicating the bookmark was not found.
    /// </returns>
    public string BookmarkNotFound()
    {
        return localizer["BookmarkNotFound"];
    }

    /// <summary>
    /// Gets an error message for when a user attempts to modify a comment they do not own.
    /// </summary>
    /// <returns>
    /// An error message indicating the user can only modify their own comments.
    /// </returns>
    public string NotCommentOwner()
    {
        return localizer["NotCommentOwner"];
    }

    /// <summary>
    /// Gets an error message for when a user attempts to reply to a comment that is itself a reply.
    /// </summary>
    public string CannotReplyToReply() => localizer["CannotReplyToReply"];

    /// <summary>
    /// Gets an error message for when a comment body is required.
    /// </summary>
    public string CommentBodyRequired() => localizer["CommentBodyRequired"];

    /// <summary>
    /// Gets an error message for when a comment body exceeds the maximum length.
    /// </summary>
    /// <param name="max">The maximum allowed length.</param>
    public string CommentBodyTooLong(int max) => string.Format(localizer["CommentBodyTooLong"], max);

    /// <summary>
    /// Gets an error message for when a star rating is out of the valid range.
    /// </summary>
    public string InvalidStarRating() => localizer["InvalidStarRating"];
}
