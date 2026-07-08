using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.RemoveOrderItem;

/// <summary>
/// Contains metadata information for the remove order item route.
/// </summary>
public static class AdminRemoveOrderItemMetaField
{
    public static readonly RouteMetadata RemoveOrderItem = new(
        "AdminRemoveOrderItem",
        "Remove an item from a draft order",
        """
            Removes a commissioned content item from a Draft order and recalculates the order total.
            Only orders in Draft status can have items removed.
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
            - Returns 404 Not Found if the order or item does not exist\n
        """
    );
}
