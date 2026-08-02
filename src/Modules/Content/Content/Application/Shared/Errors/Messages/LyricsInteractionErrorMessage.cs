using Microsoft.Extensions.Localization;

namespace _116.Content.Application.Shared.Errors.Messages;

/// <summary>
/// Provides error messages for lyrics interaction operations (likes).
/// </summary>
public class LyricsInteractionErrorMessage(IStringLocalizer<LyricsInteractionErrorMessage> localizer)
{
    /// <summary>
    /// Exposes the underlying localizer for shared validation extensions.
    /// </summary>
    public IStringLocalizer Localizer => localizer;

    /// <summary>
    /// Gets an error message for when a user has already liked a lyrics page.
    /// </summary>
    /// <returns>
    /// An error message indicating the lyrics page has already been liked.
    /// </returns>
    public string AlreadyLiked()
    {
        return localizer["AlreadyLiked"];
    }

    /// <summary>
    /// Gets an error message for when a like is not found for a lyrics page.
    /// </summary>
    /// <returns>
    /// An error message indicating the like was not found.
    /// </returns>
    public string LikeNotFound()
    {
        return localizer["LikeNotFound"];
    }
}
