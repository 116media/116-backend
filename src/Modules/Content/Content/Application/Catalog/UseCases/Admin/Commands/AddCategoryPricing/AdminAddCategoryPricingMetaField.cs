using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.AddCategoryPricing;

/// <summary>
/// Contains metadata information for the add category pricing route.
/// </summary>
public static class AdminAddCategoryPricingMetaField
{
    public static readonly RouteMetadata AdminAddCategoryPricing = new(
        "AdminAddCategoryPricing",
        "Add a pricing tier to a category",
        """
            Attaches a pricing tier to a category and sets the price for that add-on
            (e.g. "Artist Profile + base_upload = USD25").
            \n
            A paid category can only accept orders once it has at least one pricing tier configured.
            The pricing tier must be active at the time of assignment.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 201 Created with the pricing details on success\n
            - Returns 400 Bad Request if validation fails\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 404 Not Found if the category or pricing tier does not exist\n
            - Returns 409 Conflict if this tier is already configured for the category, or the tier is inactive\n
        """
    );
}
