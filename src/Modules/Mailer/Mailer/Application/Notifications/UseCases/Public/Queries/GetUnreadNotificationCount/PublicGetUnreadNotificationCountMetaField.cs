using _116.Shared.Application.Metadata;

namespace _116.Mailer.Application.Notifications.UseCases.Public.Queries.GetUnreadNotificationCount;

/// <summary>
/// Contains metadata information for the unread notification count route.
/// </summary>
public static class PublicGetUnreadNotificationCountMetaField
{
    public static readonly RouteMetadata GetUnreadNotificationCount = new(
        "PublicGetUnreadNotificationCount",
        "Count the authenticated user's unread notifications",
        """
            Returns the number of the caller's notifications without a read\n
            time, for the frontend badge.\n
            \n
            **Behavior:**\n
            - Only the caller's own rows are counted\n
            - Reading a notification or the read-all action decrements the count\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with the count\n
            - Returns 401 Unauthorized without a valid token.
        """
    );
}
