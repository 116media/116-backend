using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Commerce.UseCases.Admin.Queries.GetOrderPayment;

/// <summary>
/// Contains metadata information for the get order payment route.
/// </summary>
public static class AdminGetOrderPaymentMetaField
{
    public static readonly RouteMetadata AdminGetOrderPayment = new(
        "AdminGetOrderPayment",
        "Get the payment record of an order",
        """
            Returns the payment details for a specific order, including status, proof URL,
            payment method, and verification details.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with the payment details\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
            - Returns 404 Not Found if the payment record does not exist\n
        """
    );
}
