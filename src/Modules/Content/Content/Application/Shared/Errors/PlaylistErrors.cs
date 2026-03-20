using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;

namespace _116.Content.Application.Shared.Errors;

/// <summary>
/// Playlist error factory providing simple, readable exception creation.
/// </summary>
public static class PlaylistErrors
{
    /// <summary>
    /// Throws when a playlist is not found by its identifier.
    /// </summary>
    public static NotFoundException NotFound(Guid id)
    {
        return new NotFoundException("Playlist", "id", keyValue: id);
    }

    /// <summary>
    /// Throws when a user attempts to manage a playlist they do not own.
    /// </summary>
    public static BadRequestException NotOwner()
    {
        return new BadRequestException(PlaylistErrorMessage.NotOwner());
    }

    /// <summary>
    /// Throws when a video is already present in the target playlist.
    /// </summary>
    public static ConflictException VideoAlreadyInPlaylist()
    {
        return new ConflictException(PlaylistErrorMessage.VideoAlreadyInPlaylist());
    }
}
