using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Commerce.UseCases.Admin.Queries.GetPendingPaymentOrders;

/// <summary>
/// Contains metadata information for the get pending-payment orders route.
/// </summary>
public static class AdminGetPendingPaymentOrdersMetaField
{
    public static readonly RouteMetadata AdminGetPendingPaymentOrders = new(
        "AdminGetPendingPaymentOrders",
        "List orders awaiting payment",
        """
            Returns a paginated list of orders in PendingPayment status, ordered oldest-first
            so that staff can process payments in the order they were submitted.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with a paginated list of order summaries\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
        """
    );
}
