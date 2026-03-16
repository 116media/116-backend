namespace _116.Content.Application.Shared.Errors.Messages;

/// <summary>
/// Provides error messages for the <c>Lyrics</c> domain.
/// Covers conflict situations and validation failures related to lyrics operations.
/// </summary>
public static class LyricsErrorMessage
{
    /// <summary>
    /// Gets an error message for when a lyrics entry already exists for the given song and artist.
    /// </summary>
    /// <param name="songTitle">The song title that caused the conflict.</param>
    /// <param name="artistName">The artist name that caused the conflict.</param>
    /// <returns>
    /// A formatted error message indicating that lyrics for the specified song and artist already exist.
    /// </returns>
    public static string AlreadyExists(string songTitle, string artistName)
    {
        return $"Lyrics for '{songTitle}' by '{artistName}' already exist";
    }

    /// <summary>
    /// Gets an error message for when a song title is required but not provided.
    /// </summary>
    /// <returns>
    /// An error message indicating that the song title is required.
    /// </returns>
    public static string SongTitleRequired()
    {
        return "Song title is required";
    }

    /// <summary>
    /// Gets an error message for when an artist name is required but not provided.
    /// </summary>
    /// <returns>
    /// An error message indicating that the artist name is required.
    /// </returns>
    public static string ArtistNameRequired()
    {
        return "Artist name is required";
    }

    /// <summary>
    /// Gets an error message for when lyrics text is required but not provided.
    /// </summary>
    /// <returns>
    /// An error message indicating that the lyrics text is required.
    /// </returns>
    public static string LyricsTextRequired()
    {
        return "Lyrics text is required";
    }
}
