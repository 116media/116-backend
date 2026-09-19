using Microsoft.Extensions.Localization;

namespace _116.Content.Application.Shared.Errors.Messages;

/// <summary>
/// Provides error messages for short video interaction operations (likes, bookmarks).
/// </summary>
public class ShortVideoInteractionErrorMessage(IStringLocalizer<ShortVideoInteractionErrorMessage> localizer)
{
    /// <summary>
    /// Exposes the underlying localizer for shared validation extensions.
    /// </summary>
    public IStringLocalizer Localizer => localizer;

    /// <summary>
    /// Gets an error message for when a user has already liked a short video.
    /// </summary>
    /// <returns>
    /// An error message indicating the short video has already been liked.
    /// </returns>
    public string AlreadyLiked()
    {
        return localizer["AlreadyLiked"];
    }

    /// <summary>
    /// Gets an error message for when a like is not found for a short video.
    /// </summary>
    /// <returns>
    /// An error message indicating the like was not found.
    /// </returns>
    public string LikeNotFound()
    {
        return localizer["LikeNotFound"];
    }

    /// <summary>
    /// Gets an error message for when a user has already bookmarked a short video.
    /// </summary>
    /// <returns>
    /// An error message indicating the short video has already been bookmarked.
    /// </returns>
    public string AlreadyBookmarked()
    {
        return localizer["AlreadyBookmarked"];
    }

    /// <summary>
    /// Gets an error message for when a bookmark is not found for a short video.
    /// </summary>
    /// <returns>
    /// An error message indicating the bookmark was not found.
    /// </returns>
    public string BookmarkNotFound()
    {
        return localizer["BookmarkNotFound"];
    }

    /// <summary>
    /// Gets an error message for when a view event's device identifier is too long.
    /// </summary>
    /// <param name="max">The maximum allowed length.</param>
    /// <returns>
    /// An error message naming the maximum length.
    /// </returns>
    public string ViewDeviceIdTooLong(int max)
    {
        return string.Format(localizer["ViewDeviceIdTooLong"], max);
    }

    /// <summary>
    /// Gets an error message for when a view event's IP address is too long.
    /// </summary>
    /// <param name="max">The maximum allowed length.</param>
    /// <returns>
    /// An error message naming the maximum length.
    /// </returns>
    public string ViewIpAddressTooLong(int max)
    {
        return string.Format(localizer["ViewIpAddressTooLong"], max);
    }

    /// <summary>
    /// Gets an error message for when a view event's user agent is too long.
    /// </summary>
    /// <param name="max">The maximum allowed length.</param>
    /// <returns>
    /// An error message naming the maximum length.
    /// </returns>
    public string ViewUserAgentTooLong(int max)
    {
        return string.Format(localizer["ViewUserAgentTooLong"], max);
    }
}
