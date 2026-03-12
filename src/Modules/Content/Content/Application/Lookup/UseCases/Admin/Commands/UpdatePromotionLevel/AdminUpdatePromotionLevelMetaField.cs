using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.UpdatePromotionLevel;

/// <summary>
/// Contains metadata information for the update promotion level route.
/// </summary>
public static class AdminUpdatePromotionLevelMetaField
{
    public static readonly RouteMetadata AdminUpdatePromotionLevel = new(
        "AdminUpdatePromotionLevel",
        "Update a promotion level",
        """
            Updates the name, duration, and price of an existing promotion level.
            \n
            Price changes on existing order snapshots are unaffected — only new orders will reflect the change.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with updated promotion level details on success\n
            - Returns 400 Bad Request if validation fails\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 404 Not Found if the promotion level does not exist\n
            - Returns 409 Conflict if the new name is already taken\n
        """
    );
}
