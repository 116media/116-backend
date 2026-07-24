using Microsoft.Extensions.Localization;

namespace _116.Content.Application.Shared.Errors.Messages;

/// <summary>
/// Provides error messages for the <c>Artist</c> domain.
/// Covers conflict situations and validation failures related to artist profile operations.
/// </summary>
public class ArtistErrorMessage(IStringLocalizer<ArtistErrorMessage> localizer)
{
    /// <summary>
    /// Exposes the underlying localizer for shared validation extensions.
    /// </summary>
    public IStringLocalizer Localizer => localizer;

    /// <summary>
    /// Gets an error message for when an artist name is required but not provided.
    /// </summary>
    /// <returns>
    /// An error message indicating that the artist name is required.
    /// </returns>
    public string NameRequired()
    {
        return localizer["NameRequired"];
    }

    /// <summary>
    /// Gets an error message for when an artist slug is required but not provided.
    /// </summary>
    /// <returns>
    /// An error message indicating that the artist slug is required.
    /// </returns>
    public string SlugRequired()
    {
        return localizer["SlugRequired"];
    }

    /// <summary>
    /// Gets an error message for when an artist profile with the given slug already exists.
    /// </summary>
    /// <param name="slug">The artist slug that caused the conflict.</param>
    /// <returns>
    /// A formatted error message indicating that an artist profile with the specified slug already exists.
    /// </returns>
    public string SlugAlreadyExists(string slug)
    {
        return string.Format(localizer["SlugAlreadyExists"], slug);
    }

    /// <summary>
    /// Gets an error message for when an artist profile has already been claimed by a
    /// verified account.
    /// </summary>
    /// <returns>
    /// An error message indicating that the artist profile is already claimed.
    /// </returns>
    public string AlreadyClaimed()
    {
        return localizer["AlreadyClaimed"];
    }
}
