using Microsoft.Extensions.Localization;

namespace _116.Content.Application.Shared.Errors.Messages;

/// <summary>
/// Provides error messages for playlist operations.
/// </summary>
public class PlaylistErrorMessage(IStringLocalizer<PlaylistErrorMessage> localizer)
{
    /// <summary>
    /// Exposes the underlying localizer for shared validation extensions.
    /// </summary>
    public IStringLocalizer Localizer => localizer;

    /// <summary>
    /// Gets an error message for when a playlist is not found by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the playlist that was not found.</param>
    /// <returns>
    /// A formatted error message indicating the playlist was not found.
    /// </returns>
    public string NotFound(Guid id)
    {
        return string.Format(localizer["NotFound"], id);
    }

    /// <summary>
    /// Gets an error message for when a user attempts to manage a playlist they do not own.
    /// </summary>
    /// <returns>
    /// An error message indicating the user can only manage their own playlists.
    /// </returns>
    public string NotOwner()
    {
        return localizer["NotOwner"];
    }

    /// <summary>
    /// Gets an error message for when a video is already present in the target playlist.
    /// </summary>
    /// <returns>
    /// An error message indicating the video is already in the playlist.
    /// </returns>
    public string VideoAlreadyInPlaylist()
    {
        return localizer["VideoAlreadyInPlaylist"];
    }

    /// <summary>
    /// Gets an error message for when a playlist name is required.
    /// </summary>
    public string NameRequired() => localizer["NameRequired"];

    /// <summary>
    /// Gets an error message for when a playlist name exceeds the maximum length.
    /// </summary>
    /// <param name="max">The maximum allowed length.</param>
    public string NameTooLong(int max) => string.Format(localizer["NameTooLong"], max);
}
