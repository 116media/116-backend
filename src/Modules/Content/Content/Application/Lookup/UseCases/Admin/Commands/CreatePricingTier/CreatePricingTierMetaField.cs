using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.CreatePricingTier;

/// <summary>
/// Contains metadata information for the create pricing tier route.
/// </summary>
public static class CreatePricingTierMetaField
{
    public static readonly RouteMetadata CreatePricingTier = new(
        "CreatePricingTier",
        "Create a new pricing tier",
        """
            Creates a new add-on service fee tier (e.g. "base_upload", "social_boost").
            \n
            This endpoint creates a pricing tier by:\n
            - Validating the tier name and optional description\n
            - Checking that no pricing tier with the same name already exists\n
            - Creating the tier with active status\n
            - Returning the created pricing tier details\n
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Request Body:**\n
            - name: The unique name for the pricing tier (max 40 characters)\n
            - description: Optional description of what this tier covers (max 200 characters)\n
            \n
            **Response Codes:**\n
            - Returns 201 Created with pricing tier details on success\n
            - Returns 400 Bad Request if validation fails\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 409 Conflict if pricing tier name already exists\n
        """
    );
}
