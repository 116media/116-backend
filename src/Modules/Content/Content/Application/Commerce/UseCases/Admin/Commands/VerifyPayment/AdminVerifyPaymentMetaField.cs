using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.VerifyPayment;

/// <summary>
/// Contains metadata information for the verify payment route.
/// </summary>
public static class AdminVerifyPaymentMetaField
{
    public static readonly RouteMetadata AdminVerifyPayment = new(
        "AdminVerifyPayment",
        "Verify an order payment",
        """
            Verifies a PendingPayment order's payment, transitioning the order to Paid status.
            A receipt URL is recorded and social boost / featured promotion is stamped on any
            already-linked content items.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 204 No Content on success\n
            - Returns 400 Bad Request if validation fails\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
            - Returns 404 Not Found if the order or payment does not exist\n
            - Returns 409 Conflict if the payment has already been verified or rejected\n
        """
    );
}
