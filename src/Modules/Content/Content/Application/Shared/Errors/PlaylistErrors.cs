using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;

namespace _116.Content.Application.Shared.Errors;

/// <summary>
/// Playlist error factory providing simple, readable exception creation.
/// </summary>
public class PlaylistErrors(PlaylistErrorMessage i18n)
{
    /// <summary>
    /// Exposes the localized message provider for use in validator extensions.
    /// </summary>
    public PlaylistErrorMessage Msg => i18n;

    /// <summary>
    /// Throws when a playlist is not found by its identifier.
    /// </summary>
    public NotFoundException NotFound(Guid id)
    {
        return new NotFoundException(i18n.NotFound(id));
    }

    /// <summary>
    /// Throws when a user attempts to manage a playlist they do not own.
    /// </summary>
    public BadRequestException NotOwner()
    {
        return new BadRequestException(i18n.NotOwner());
    }

    /// <summary>
    /// Throws when a video is already present in the target playlist.
    /// </summary>
    public ConflictException VideoAlreadyInPlaylist()
    {
        return new ConflictException(i18n.VideoAlreadyInPlaylist());
    }
}
