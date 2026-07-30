using _116.Shared.Application.Metadata;

namespace _116.Mailer.Application.Notifications.UseCases.Public.Commands.MarkAllNotificationsRead;

/// <summary>
/// Contains metadata information for the mark-all-notifications-read route.
/// </summary>
public static class PublicMarkAllNotificationsReadMetaField
{
    public static readonly RouteMetadata MarkAllNotificationsRead = new(
        "PublicMarkAllNotificationsRead",
        "Mark every unread notification of the authenticated user read",
        """
            Sets the read time on all of the caller's notifications that have\n
            none, in one call.\n
            \n
            **Behavior:**\n
            - Only the caller's own rows are affected\n
            - Already read rows keep their original read time\n
            - A second call finds nothing unread and marks zero rows (idempotent)\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with the number of rows marked by this call\n
            - Returns 401 Unauthorized without a valid token.
        """
    );
}
