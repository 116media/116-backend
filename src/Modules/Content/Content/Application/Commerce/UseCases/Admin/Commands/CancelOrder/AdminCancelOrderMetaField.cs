using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.CancelOrder;

/// <summary>
/// Contains metadata information for the cancel order route.
/// </summary>
public static class AdminCancelOrderMetaField
{
    public static readonly RouteMetadata AdminCancelOrder = new(
        "AdminCancelOrder",
        "Cancel a draft or pending-payment order",
        """
            Cancels a Draft or PendingPayment order. Paid orders cannot be cancelled.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on success\n
            - Returns 400 Bad Request if the order is Paid and cannot be cancelled\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
            - Returns 404 Not Found if the order does not exist\n
            - Returns 409 Conflict if the order is already cancelled\n
        """
    );
}
