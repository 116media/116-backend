using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;

namespace _116.Content.Application.Shared.Errors;

/// <summary>
/// Short video interaction error factory providing simple, readable exception creation.
/// Covers likes and bookmarks on short videos.
/// </summary>
public static class ShortVideoInteractionErrors
{
    /// <summary>
    /// Throws when a user attempts to like a short video they have already liked.
    /// </summary>
    public static ConflictException AlreadyLiked()
    {
        return new ConflictException(ShortVideoInteractionErrorMessage.AlreadyLiked());
    }

    /// <summary>
    /// Throws when a like is not found for the given short video and user.
    /// </summary>
    public static BadRequestException LikeNotFound()
    {
        return new BadRequestException(ShortVideoInteractionErrorMessage.LikeNotFound());
    }

    /// <summary>
    /// Throws when a user attempts to bookmark a short video they have already bookmarked.
    /// </summary>
    public static ConflictException AlreadyBookmarked()
    {
        return new ConflictException(ShortVideoInteractionErrorMessage.AlreadyBookmarked());
    }

    /// <summary>
    /// Throws when a bookmark is not found for the given short video and user.
    /// </summary>
    public static BadRequestException BookmarkNotFound()
    {
        return new BadRequestException(ShortVideoInteractionErrorMessage.BookmarkNotFound());
    }
}
