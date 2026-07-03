using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.RemoveItemTier;

/// <summary>
/// Contains metadata information for the remove item tier route.
/// </summary>
public static class AdminRemoveItemTierMetaField
{
    public static readonly RouteMetadata RemoveItemTier = new(
        "AdminRemoveItemTier",
        "Remove a pricing tier from an order item",
        """
            Removes a pricing tier snapshot from an order item in a Draft order and recalculates
            the order total. Only orders in Draft status can have tiers removed.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on success\n
            - Returns 400 Bad Request if the order is not in Draft status or validation fails\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
            - Returns 404 Not Found if the order, item, or tier does not exist\n
        """
    );
}
