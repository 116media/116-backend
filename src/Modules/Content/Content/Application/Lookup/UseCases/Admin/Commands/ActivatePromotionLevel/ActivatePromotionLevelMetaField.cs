using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.ActivatePromotionLevel;

/// <summary>
/// Contains metadata information for the activate promotion level route.
/// </summary>
public static class ActivatePromotionLevelMetaField
{
    public static readonly RouteMetadata ActivatePromotionLevel = new(
        "ActivatePromotionLevel",
        "Activate a promotion level",
        """
            Activates a promotion level, making it available for assignment to content.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 204 No Content on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 404 Not Found if the promotion level does not exist\n
            - Returns 409 Conflict if the promotion level is already active\n
        """
    );
}
