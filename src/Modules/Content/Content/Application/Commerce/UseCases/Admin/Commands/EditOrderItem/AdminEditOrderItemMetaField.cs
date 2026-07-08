using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.EditOrderItem;

/// <summary>
/// Contains metadata information for the edit order item route.
/// </summary>
public static class AdminEditOrderItemMetaField
{
    public static readonly RouteMetadata EditOrderItem = new(
        "AdminEditOrderItem",
        "Edit a content item in a draft order",
        """
            Edits an existing content item in a Draft order. Supports changing the content kind,
            category, promotion level, social boost, and bonus status.
            \n
            When the promotion level changes, the price is re-snapshotted from the current level price.
            The order total is recalculated after the update.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with the updated order item on success\n
            - Returns 400 Bad Request if the order is not in Draft status or validation fails\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
            - Returns 404 Not Found if the order, item, category, or promotion level does not exist\n
        """
    );
}
