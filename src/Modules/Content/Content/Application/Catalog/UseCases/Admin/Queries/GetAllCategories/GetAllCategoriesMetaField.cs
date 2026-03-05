using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Catalog.UseCases.Admin.Queries.GetAllCategories;

/// <summary>
/// Contains metadata information for the get all categories route.
/// </summary>
public static class GetAllCategoriesMetaField
{
    public static readonly RouteMetadata GetAllCategories = new(
        "GetAllCategories",
        "List all categories",
        """
            Returns a paginated list of all content categories.
            \n
            Supports filtering by active status and free/paid status.
            Used by the admin to review what content formats are currently configured,
            identify categories that still need pricing, and find active paid categories.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with paginated category list on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
        """
    );
}
