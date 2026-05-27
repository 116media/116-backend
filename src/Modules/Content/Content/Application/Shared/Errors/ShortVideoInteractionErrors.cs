using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;

namespace _116.Content.Application.Shared.Errors;

/// <summary>
/// Short video interaction error factory providing simple, readable exception creation.
/// Covers likes and bookmarks on short videos.
/// </summary>
public class ShortVideoInteractionErrors(ShortVideoInteractionErrorMessage i18n)
{
    /// <summary>
    /// Throws when a user attempts to like a short video they have already liked.
    /// </summary>
    public ConflictException AlreadyLiked()
    {
        return new ConflictException(i18n.AlreadyLiked());
    }

    /// <summary>
    /// Throws when a like is not found for the given short video and user.
    /// </summary>
    public BadRequestException LikeNotFound()
    {
        return new BadRequestException(i18n.LikeNotFound());
    }

    /// <summary>
    /// Throws when a user attempts to bookmark a short video they have already bookmarked.
    /// </summary>
    public ConflictException AlreadyBookmarked()
    {
        return new ConflictException(i18n.AlreadyBookmarked());
    }

    /// <summary>
    /// Throws when a bookmark is not found for the given short video and user.
    /// </summary>
    public BadRequestException BookmarkNotFound()
    {
        return new BadRequestException(i18n.BookmarkNotFound());
    }
}
