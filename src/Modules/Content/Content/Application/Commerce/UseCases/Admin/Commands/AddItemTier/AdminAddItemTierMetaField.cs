using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.AddItemTier;

/// <summary>
/// Contains metadata information for the add item tier route.
/// </summary>
public static class AdminAddItemTierMetaField
{
    public static readonly RouteMetadata AdminAddItemTier = new(
        "AdminAddItemTier",
        "Attach a pricing tier to an order item",
        """
            Attaches a pricing tier snapshot to a specific order item in a Draft order.
            The category pricing is snapshotted at this moment so the client's quote is locked
            even if the admin adjusts prices later.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 201 Created with the tier snapshot details on success\n
            - Returns 400 Bad Request if the order is not in Draft status or validation fails\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
            - Returns 404 Not Found if the order, item, or pricing tier does not exist\n
        """
    );
}
