using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.AddOrderItem;

/// <summary>
/// Contains metadata information for the add order item route.
/// </summary>
public static class AdminAddOrderItemMetaField
{
    public static readonly RouteMetadata AdminAddOrderItem = new(
        "AdminAddOrderItem",
        "Add a content item to an order",
        """
            Adds one commissioned content item to a Draft order. Each item specifies the category,
            the kind of content (Article or Video), and optional promotion options.
            \n
            The promotion level price is snapshotted at this moment so the client's quote is locked
            even if the admin adjusts prices later.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 201 Created with the order item details on success\n
            - Returns 400 Bad Request if the order is not in Draft status or validation fails\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
            - Returns 404 Not Found if the order, category, or promotion level does not exist\n
        """
    );
}
