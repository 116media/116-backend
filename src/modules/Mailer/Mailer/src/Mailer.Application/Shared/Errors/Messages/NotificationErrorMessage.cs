using Microsoft.Extensions.Localization;

namespace _116.Mailer.Application.Shared.Errors.Messages;

/// <summary>
/// Provides error messages for the in-app notification feed.
/// Covers row lookup and mark-read input validation.
/// </summary>
public class NotificationErrorMessage(IStringLocalizer<NotificationErrorMessage> localizer)
{
    /// <summary>
    /// Gets an error message for when a notification does not exist for the
    /// requesting user. Used for both unknown ids and rows owned by another
    /// user, so the response never leaks row existence.
    /// </summary>
    /// <returns>
    /// An error message indicating the notification was not found.
    /// </returns>
    public string NotificationNotFound()
    {
        return localizer["NotificationNotFound"];
    }

    /// <summary>
    /// Gets an error message for when the notification identifier is missing.
    /// </summary>
    /// <returns>
    /// An error message indicating the notification identifier is required.
    /// </returns>
    public string NotificationIdRequired()
    {
        return localizer["NotificationIdRequired"];
    }
}
