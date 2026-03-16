using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.RemoveCategoryPricing;

/// <summary>
/// Contains metadata information for the remove category pricing route.
/// </summary>
public static class AdminRemoveCategoryPricingMetaField
{
    public static readonly RouteMetadata AdminRemoveCategoryPricing = new(
        "AdminRemoveCategoryPricing",
        "Remove a pricing tier from a category",
        """
            Removes a pricing tier from a category when that add-on service is no longer offered for that content type.
            \n
            **Note:** Existing orders that already contain this tier are unaffected —
            the price snapshot is preserved on the order item tier record.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 204 No Content on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 404 Not Found if the pricing configuration does not exist\n
        """
    );
}
