using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Catalog.UseCases.Admin.Queries.GetCategoryById;

/// <summary>
/// Contains metadata information for the get category by id route.
/// </summary>
public static class GetCategoryByIdMetaField
{
    public static readonly RouteMetadata GetCategoryById = new(
        "GetCategoryById",
        "Get a category by ID",
        """
            Returns the full details of a single category, including its complete pricing configuration.
            \n
            Used by the admin when setting up an order — they need to know exactly what pricing tiers
            are available for a category before adding items to an order.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with category details on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
            - Returns 404 Not Found if the category does not exist\n
        """
    );
}
