using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;

namespace _116.Content.Application.Shared.Errors;

/// <summary>
/// Short video interaction error factory providing simple, readable exception creation.
/// Covers likes and bookmarks on short videos.
/// </summary>
public class ShortVideoInteractionErrors(ShortVideoInteractionErrorMessage msg)
{
    /// <summary>
    /// Throws when a user attempts to like a short video they have already liked.
    /// </summary>
    public ConflictException AlreadyLiked()
    {
        return new ConflictException(msg.AlreadyLiked());
    }

    /// <summary>
    /// Throws when a like is not found for the given short video and user.
    /// </summary>
    public BadRequestException LikeNotFound()
    {
        return new BadRequestException(msg.LikeNotFound());
    }

    /// <summary>
    /// Throws when a user attempts to bookmark a short video they have already bookmarked.
    /// </summary>
    public ConflictException AlreadyBookmarked()
    {
        return new ConflictException(msg.AlreadyBookmarked());
    }

    /// <summary>
    /// Throws when a bookmark is not found for the given short video and user.
    /// </summary>
    public BadRequestException BookmarkNotFound()
    {
        return new BadRequestException(msg.BookmarkNotFound());
    }
}
