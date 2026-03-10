using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.DeactivateCategory;

/// <summary>
/// Contains metadata information for the deactivate category route.
/// </summary>
public static class DeactivateCategoryMetaField
{
    public static readonly RouteMetadata DeactivateCategory = new(
        "DeactivateCategory",
        "Deactivate a category",
        """
            Deactivates a category, preventing it from being used in new content or orders.
            \n
            Existing content assigned to this category is not affected.
            The category can be restored later using the activate endpoint.
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
            - Returns 409 Conflict if the category is already inactive\n
        """
    );
}
