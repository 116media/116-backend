using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.ActivatePricingTier;

/// <summary>
/// Contains metadata information for the activate pricing tier route.
/// </summary>
public static class AdminActivatePricingTierMetaField
{
    public static readonly RouteMetadata ActivatePricingTier = new(
        "ActivatePricingTier",
        "Activate a pricing tier",
        """
            Activates a pricing tier, making it available for assignment to content.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 404 Not Found if the pricing tier does not exist\n
            - Returns 409 Conflict if the pricing tier is already active\n
        """
    );
}
