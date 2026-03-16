using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.ActivateCategory;

/// <summary>
/// Contains metadata information for the activate category route.
/// </summary>
public static class AdminActivateCategoryMetaField
{
    public static readonly RouteMetadata AdminActivateCategory = new(
        "AdminActivateCategory",
        "Activate a category",
        """
            Activates a category, making it available for content creation and orders.
            \n
            An inactive category cannot be used in new content or order items.
            This operation restores it to active status.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with updated category details on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
            - Returns 404 Not Found if the category does not exist\n
            - Returns 409 Conflict if the category is already active\n
        """
    );
}
