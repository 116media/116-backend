namespace _116.Content.Application.Shared.Errors.Messages;

/// <summary>
/// Provides error messages for playlist operations.
/// </summary>
public static class PlaylistErrorMessage
{
    /// <summary>
    /// Gets an error message for when a playlist is not found by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the playlist that was not found.</param>
    /// <returns>
    /// A formatted error message indicating the playlist was not found.
    /// </returns>
    public static string NotFound(Guid id)
    {
        return $"Playlist '{id}' not found";
    }

    /// <summary>
    /// Gets an error message for when a user attempts to manage a playlist they do not own.
    /// </summary>
    /// <returns>
    /// An error message indicating the user can only manage their own playlists.
    /// </returns>
    public static string NotOwner()
    {
        return "You can only manage your own playlists";
    }

    /// <summary>
    /// Gets an error message for when a video is already present in the target playlist.
    /// </summary>
    /// <returns>
    /// An error message indicating the video is already in the playlist.
    /// </returns>
    public static string VideoAlreadyInPlaylist()
    {
        return "This video is already in the playlist";
    }
}
