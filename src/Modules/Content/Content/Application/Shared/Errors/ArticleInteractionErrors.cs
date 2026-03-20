using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;

namespace _116.Content.Application.Shared.Errors;

/// <summary>
/// Article interaction error factory providing simple, readable exception creation.
/// Covers likes, bookmarks, and comments on articles.
/// </summary>
public static class ArticleInteractionErrors
{
    /// <summary>
    /// Throws when a user attempts to like an article they have already liked.
    /// </summary>
    public static ConflictException AlreadyLiked()
    {
        return new ConflictException(ArticleInteractionErrorMessage.AlreadyLiked());
    }

    /// <summary>
    /// Throws when a like is not found for the given article and user.
    /// </summary>
    public static BadRequestException LikeNotFound()
    {
        return new BadRequestException(ArticleInteractionErrorMessage.LikeNotFound());
    }

    /// <summary>
    /// Throws when a user attempts to bookmark an article they have already bookmarked.
    /// </summary>
    public static ConflictException AlreadyBookmarked()
    {
        return new ConflictException(ArticleInteractionErrorMessage.AlreadyBookmarked());
    }

    /// <summary>
    /// Throws when a bookmark is not found for the given article and user.
    /// </summary>
    public static BadRequestException BookmarkNotFound()
    {
        return new BadRequestException(ArticleInteractionErrorMessage.BookmarkNotFound());
    }

    /// <summary>
    /// Throws when a comment is not found by its identifier.
    /// </summary>
    public static NotFoundException CommentNotFound(Guid commentId)
    {
        return new NotFoundException("ArticleComment", "id", keyValue: commentId);
    }

    /// <summary>
    /// Throws when a user attempts to modify a comment they do not own.
    /// </summary>
    public static BadRequestException NotCommentOwner()
    {
        return new BadRequestException(ArticleInteractionErrorMessage.NotCommentOwner());
    }
}
