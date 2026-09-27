using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Shared.Errors.Messages;

namespace _116.Content.Application.Shared.Errors;

/// <summary>
/// Lyrics interaction error factory providing simple, readable exception creation.
/// Covers likes on lyrics pages.
/// </summary>
public class LyricsInteractionErrors(LyricsInteractionErrorMessage i18n)
{
    /// <summary>
    /// Exposes the localized message provider for use in validator extensions.
    /// </summary>
    public LyricsInteractionErrorMessage Msg => i18n;

    /// <summary>
    /// Throws when a user attempts to like a lyrics page they have already liked.
    /// </summary>
    public ConflictException AlreadyLiked()
    {
        return new ConflictException(i18n.AlreadyLiked());
    }

    /// <summary>
    /// Throws when a like is not found for the given lyrics page and user.
    /// </summary>
    public BadRequestException LikeNotFound()
    {
        return new BadRequestException(i18n.LikeNotFound());
    }
}
