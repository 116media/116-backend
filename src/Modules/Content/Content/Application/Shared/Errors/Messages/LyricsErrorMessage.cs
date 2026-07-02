using Microsoft.Extensions.Localization;

namespace _116.Content.Application.Shared.Errors.Messages;

/// <summary>
/// Provides error messages for the <c>Lyrics</c> domain.
/// Covers conflict situations and validation failures related to lyrics operations.
/// </summary>
public class LyricsErrorMessage(IStringLocalizer<LyricsErrorMessage> localizer)
{
    /// <summary>
    /// Exposes the underlying localizer for shared validation extensions.
    /// </summary>
    public IStringLocalizer Localizer => localizer;

    /// <summary>
    /// Gets an error message for when a lyrics entry already exists for the given song and artist.
    /// </summary>
    /// <param name="songTitle">The song title that caused the conflict.</param>
    /// <param name="artistName">The artist name that caused the conflict.</param>
    /// <returns>
    /// A formatted error message indicating that lyrics for the specified song and artist already exist.
    /// </returns>
    public string AlreadyExists(string songTitle, string artistName)
    {
        return string.Format(localizer["AlreadyExists"], songTitle, artistName);
    }

    /// <summary>
    /// Gets an error message for when a song title is required but not provided.
    /// </summary>
    /// <returns>
    /// An error message indicating that the song title is required.
    /// </returns>
    public string SongTitleRequired()
    {
        return localizer["SongTitleRequired"];
    }

    /// <summary>
    /// Gets an error message for when an artist name is required but not provided.
    /// </summary>
    /// <returns>
    /// An error message indicating that the artist name is required.
    /// </returns>
    public string ArtistNameRequired()
    {
        return localizer["ArtistNameRequired"];
    }

    /// <summary>
    /// Gets an error message for when lyrics text is required but not provided.
    /// </summary>
    /// <returns>
    /// An error message indicating that the lyrics text is required.
    /// </returns>
    public string LyricsTextRequired()
    {
        return localizer["LyricsTextRequired"];
    }

    /// <summary>
    /// Gets an error message for when a song title exceeds the maximum length.
    /// </summary>
    /// <param name="max">The maximum allowed length.</param>
    public string SongTitleTooLong(int max) => string.Format(localizer["SongTitleTooLong"], max);

    /// <summary>
    /// Gets an error message for when an artist name exceeds the maximum length.
    /// </summary>
    /// <param name="max">The maximum allowed length.</param>
    public string ArtistNameTooLong(int max) => string.Format(localizer["ArtistNameTooLong"], max);

    /// <summary>
    /// Gets an error message for when lyrics language is required.
    /// </summary>
    public string LanguageRequired() => localizer["LanguageRequired"];

    /// <summary>
    /// Gets an error message for when lyrics language exceeds the maximum length.
    /// </summary>
    /// <param name="max">The maximum allowed length.</param>
    public string LanguageTooLong(int max) => string.Format(localizer["LanguageTooLong"], max);
}
