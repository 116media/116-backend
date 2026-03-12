using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Lookup.UseCases.Admin.Queries.GetAllPricingTiers;

/// <summary>
/// Contains metadata information for the get all pricing tiers route.
/// </summary>
public static class AdminGetAllPricingTiersMetaField
{
    public static readonly RouteMetadata AdminGetAllPricingTiers = new(
        "AdminGetAllPricingTiers",
        "List all pricing tiers",
        """
            Returns the complete list of pricing tiers available for category pricing configuration.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with the list of pricing tiers on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
        """
    );
}
