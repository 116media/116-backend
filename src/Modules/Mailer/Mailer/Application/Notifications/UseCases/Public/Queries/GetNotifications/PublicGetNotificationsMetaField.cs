using _116.Shared.Application.Metadata;

namespace _116.Mailer.Application.Notifications.UseCases.Public.Queries.GetNotifications;

/// <summary>
/// Contains metadata information for the notification feed route.
/// </summary>
public static class PublicGetNotificationsMetaField
{
    public static readonly RouteMetadata GetNotifications = new(
        "PublicGetNotifications",
        "List the authenticated user's notifications",
        """
            Returns the caller's notification feed, newest first, paginated.\n
            Title and body are pre-rendered in the culture active when the\n
            notification was written.\n
            \n
            **Behavior:**\n
            - Only the caller's own rows are returned\n
            - `unreadOnly=true` filters to rows without a read time\n
            - `pageIndex` is zero-based; `pageSize` is clamped to 1..100\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with the paginated feed\n
            - Returns 401 Unauthorized without a valid token.
        """
    );
}
