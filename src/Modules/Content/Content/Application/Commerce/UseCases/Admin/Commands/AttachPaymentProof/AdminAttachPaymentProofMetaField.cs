using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.AttachPaymentProof;

/// <summary>
/// Contains metadata information for the attach payment proof route.
/// </summary>
public static class AdminAttachPaymentProofMetaField
{
    public static readonly RouteMetadata AdminAttachPaymentProof = new(
        "AdminAttachPaymentProof",
        "Attach a payment proof to an order",
        """
            Uploads the customer's payment receipt image to Cloudinary and attaches the resulting
            URL together with the payment method to the order's payment record.
            Accepts a multipart/form-data request with the image file and payment method.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on success\n
            - Returns 400 Bad Request if validation fails\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
            - Returns 404 Not Found if the payment record does not exist\n
        """
    );
}
