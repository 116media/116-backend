using Microsoft.Extensions.Localization;

namespace _116.Content.Application.Shared.Errors.Messages;

/// <summary>
/// Provides error messages for the <c>Album</c> domain.
/// Covers validation failures related to album operations.
/// </summary>
public class AlbumErrorMessage(IStringLocalizer<AlbumErrorMessage> localizer)
{
    /// <summary>
    /// Exposes the underlying localizer for shared validation extensions.
    /// </summary>
    public IStringLocalizer Localizer => localizer;

    /// <summary>
    /// Gets an error message for when an album name is required but not provided.
    /// </summary>
    /// <returns>
    /// An error message indicating that the album name is required.
    /// </returns>
    public string NameRequired()
    {
        return localizer["NameRequired"];
    }
}
