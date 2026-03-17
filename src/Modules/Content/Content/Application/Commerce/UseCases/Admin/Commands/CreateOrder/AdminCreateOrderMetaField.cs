using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.CreateOrder;

/// <summary>
/// Contains metadata information for the create order route.
/// </summary>
public static class AdminCreateOrderMetaField
{
    public static readonly RouteMetadata AdminCreateOrder = new(
        "AdminCreateOrder",
        "Create a new content order",
        """
            Opens a new content order for a B2B client. This is the first step in the revenue flow —
            before any commissioned article or video can be created, an order must exist that links the
            work to the customer who is paying for it.
            \n
            The order starts in Draft status with no items or total yet. The admin adds items and tiers
            before submitting. Optionally linking a package applies a pre-configured bundle deal.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 201 Created with the order summary on success\n
            - Returns 400 Bad Request if validation fails\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
            - Returns 404 Not Found if the customer or package does not exist\n
        """
    );
}
