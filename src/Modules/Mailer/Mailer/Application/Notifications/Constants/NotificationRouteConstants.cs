namespace _116.Mailer.Application.Notifications.Constants;

/// <summary>
/// Contains route path constants for in-app notification API endpoints.
/// </summary>
public static class NotificationRouteConstants
{
    /// <summary>
    /// Route segment for the notification collection.
    /// Example: /api/v1/public/notifications.
    /// </summary>
    public const string Root = "/";

    /// <summary>
    /// Route segment for the unread counter.
    /// Example: /api/v1/public/notifications/unread-count.
    /// </summary>
    public const string UnreadCount = "unread-count";

    /// <summary>
    /// Route segment marking a single notification read.
    /// Example: /api/v1/public/notifications/{id}/read.
    /// </summary>
    public const string Read = "{id:guid}/read";

    /// <summary>
    /// Route segment marking every notification read.
    /// Example: /api/v1/public/notifications/read-all.
    /// </summary>
    public const string ReadAll = "read-all";
}
