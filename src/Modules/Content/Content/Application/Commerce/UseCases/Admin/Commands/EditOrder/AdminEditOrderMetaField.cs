using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.EditOrder;

/// <summary>
/// Contains metadata information for the edit order route.
/// </summary>
public static class AdminEditOrderMetaField
{
    public static readonly RouteMetadata EditOrder = new(
        "AdminEditOrder",
        "Edit a draft order",
        """
            Edits a Draft content order's customer or package assignment.
            Only orders in Draft status can be modified.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with the updated order summary on success\n
            - Returns 400 Bad Request if the order is not in Draft status or validation fails\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
            - Returns 404 Not Found if the order or customer does not exist\n
        """
    );
}
