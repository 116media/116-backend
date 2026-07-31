using _116.Content.Domain.Constants;
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

    /// <summary>
    /// Gets an error message for when the requesting account has already filed an ownership
    /// claim for the same artist profile.
    /// </summary>
    /// <returns>
    /// An error message indicating that a claim request for this profile is already on file.
    /// </returns>
    public string ClaimRequestAlreadyExists()
    {
        return localizer["ClaimRequestAlreadyExists"];
    }

    /// <summary>
    /// Gets an error message for when an artist profile carries too many alternate names.
    /// </summary>
    /// <returns>
    /// A formatted error message stating the maximum number of aliases allowed.
    /// </returns>
    public string TooManyAliases()
    {
        return string.Format(localizer["TooManyAliases"], ContentConstants.MaxArtistAliasCount);
    }

    /// <summary>
    /// Gets an error message for when a single alternate name is too long.
    /// </summary>
    /// <returns>
    /// A formatted error message stating the maximum alias length allowed.
    /// </returns>
    public string AliasTooLong()
    {
        return string.Format(localizer["AliasTooLong"], ContentConstants.MaxArtistNameLength);
    }

    /// <summary>
    /// Gets an error message for when an artist birthdate is not in the past.
    /// </summary>
    /// <returns>
    /// An error message indicating that the birthdate must be in the past.
    /// </returns>
    public string BirthdateInFuture()
    {
        return localizer["BirthdateInFuture"];
    }

    /// <summary>
    /// Gets an error message for when a directory request combines a letter filter with a
    /// search term.
    /// </summary>
    /// <returns>
    /// An error message indicating that the two filters are mutually exclusive.
    /// </returns>
    public string LetterAndSearchExclusive()
    {
        return localizer["LetterAndSearchExclusive"];
    }
}
