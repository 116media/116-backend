using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCategoryPricing;

/// <summary>
/// Contains metadata information for the update category pricing route.
/// </summary>
public static class UpdateCategoryPricingMetaField
{
    public static readonly RouteMetadata UpdateCategoryPricing = new(
        "UpdateCategoryPricing",
        "Update a category pricing tier price",
        """
            Updates the price for a specific pricing tier within a category.
            \n
            **Note:** Price changes apply only to future orders — existing order items have their
            price frozen at snapshot time and are never retroactively affected.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with the updated pricing details on success\n
            - Returns 400 Bad Request if validation fails\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 404 Not Found if the category or pricing tier is not configured\n
        """
    );
}
