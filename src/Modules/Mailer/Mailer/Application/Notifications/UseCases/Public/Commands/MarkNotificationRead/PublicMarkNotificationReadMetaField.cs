using _116.Shared.Application.Metadata;

namespace _116.Mailer.Application.Notifications.UseCases.Public.Commands.MarkNotificationRead;

/// <summary>
/// Contains metadata information for the mark-notification-read route.
/// </summary>
public static class PublicMarkNotificationReadMetaField
{
    public static readonly RouteMetadata MarkNotificationRead = new(
        "PublicMarkNotificationRead",
        "Mark one of the authenticated user's notifications read",
        """
            Sets the read time on a single notification owned by the caller.\n
            \n
            **Behavior:**\n
            - First call: the notification becomes read\n
            - Re-call: succeeds and keeps the original read time (idempotent)\n
            - Unknown id or another user's row: 404 problem, indistinguishable\n
              by design so row existence never leaks\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with the read state\n
            - Returns 404 Not Found when the notification does not exist for the caller\n
            - Returns 401 Unauthorized without a valid token.
        """
    );
}
