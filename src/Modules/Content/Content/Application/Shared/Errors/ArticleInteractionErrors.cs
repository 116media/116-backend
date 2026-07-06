using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;

namespace _116.Content.Application.Shared.Errors;

/// <summary>
/// Article interaction error factory providing simple, readable exception creation.
/// Covers likes, bookmarks, and comments on articles.
/// </summary>
public class ArticleInteractionErrors(ArticleInteractionErrorMessage i18n)
{
    /// <summary>
    /// Exposes the localized message provider for use in validator extensions.
    /// </summary>
    public ArticleInteractionErrorMessage Msg => i18n;

    /// <summary>
    /// Throws when a user attempts to like an article they have already liked.
    /// </summary>
    public ConflictException AlreadyLiked()
    {
        return new ConflictException(i18n.AlreadyLiked());
    }

    /// <summary>
    /// Throws when a like is not found for the given article and user.
    /// </summary>
    public BadRequestException LikeNotFound()
    {
        return new BadRequestException(i18n.LikeNotFound());
    }

    /// <summary>
    /// Throws when a user attempts to bookmark an article they have already bookmarked.
    /// </summary>
    public ConflictException AlreadyBookmarked()
    {
        return new ConflictException(i18n.AlreadyBookmarked());
    }

    /// <summary>
    /// Throws when a bookmark is not found for the given article and user.
    /// </summary>
    public BadRequestException BookmarkNotFound()
    {
        return new BadRequestException(i18n.BookmarkNotFound());
    }

    /// <summary>
    /// Throws when a comment is not found by its identifier.
    /// </summary>
    public NotFoundException CommentNotFound(Guid commentId)
    {
        return new NotFoundException("ArticleComment", "id", keyValue: commentId);
    }

    /// <summary>
    /// Throws when a user attempts to modify a comment they do not own.
    /// </summary>
    public BadRequestException NotCommentOwner()
    {
        return new BadRequestException(i18n.NotCommentOwner());
    }

    /// <summary>
    /// Throws when a user attempts to reply to a comment that is itself a reply
    /// (only single-level threading is supported).
    /// </summary>
    public BadRequestException CannotReplyToReply()
    {
        return new BadRequestException(i18n.CannotReplyToReply());
    }
}
