using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.DeactivatePricingTier;

/// <summary>
/// Contains metadata information for the deactivate pricing tier route.
/// </summary>
public static class DeactivatePricingTierMetaField
{
    public static readonly RouteMetadata DeactivatePricingTier = new(
        "DeactivatePricingTier",
        "Deactivate a pricing tier",
        """
            Deactivates a pricing tier, preventing it from being assigned to new content.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 204 No Content on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 404 Not Found if the pricing tier does not exist\n
            - Returns 409 Conflict if the pricing tier is already inactive\n
        """
    );
}
