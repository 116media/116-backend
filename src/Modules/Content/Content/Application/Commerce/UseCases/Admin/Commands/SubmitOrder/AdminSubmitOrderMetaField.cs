using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.SubmitOrder;

/// <summary>
/// Contains metadata information for the "submit order" route.
/// </summary>
public static class AdminSubmitOrderMetaField
{
    public static readonly RouteMetadata AdminSubmitOrder = new(
        "AdminSubmitOrder",
        "Submit a draft order for payment",
        """
            Submits a Draft order, transitioning it to PendingPayment status.
            A payment record is created automatically at the order's current total amount.
            \n
            The order must have at least one item, and each item must have at least one pricing tier attached.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on success\n
            - Returns 400 Bad Request if the order is not in Draft status, or has no items with tiers\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
            - Returns 404 Not Found if the order does not exist\n
        """
    );
}
