using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.RejectPayment;

/// <summary>
/// Contains metadata information for the reject payment route.
/// </summary>
public static class AdminRejectPaymentMetaField
{
    public static readonly RouteMetadata AdminRejectPayment = new(
        "AdminRejectPayment",
        "Reject an order payment",
        """
            Rejects an order's payment with optional explanatory notes.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
            - Returns 404 Not Found if the payment record does not exist\n
            - Returns 409 Conflict if the payment has already been verified or rejected\n
        """
    );
}
