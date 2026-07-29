using _116.Shared.Application.Metadata;

namespace _116.Mailer.Application.Newsletter.UseCases.Admin.Queries.GetNewsletterSubscribers;

/// <summary>
/// Contains metadata information for the admin newsletter subscribers route.
/// </summary>
public static class AdminGetNewsletterSubscribersMetaField
{
    public static readonly RouteMetadata GetNewsletterSubscribers = new(
        "AdminGetNewsletterSubscribers",
        "List newsletter subscribers with status filter and pagination",
        """
            Returns the paginated list of newsletter subscribers, newest first.\n
            \n
            **Query Parameters:**\n
            - pageIndex: zero-based page index (default 0)\n
            - pageSize: page size (default 20, max 100)\n
            - status: optional filter - PendingConfirmation, Subscribed or Unsubscribed\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with the paginated subscribers\n
            - Returns 401 Unauthorized without a valid token\n
            - Returns 403 Forbidden for non-admin users.
        """
    );
}
