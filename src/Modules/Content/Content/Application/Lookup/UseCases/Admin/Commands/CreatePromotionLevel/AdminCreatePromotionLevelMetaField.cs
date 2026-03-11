using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.CreatePromotionLevel;

/// <summary>
/// Contains metadata information for the create promotion level route.
/// </summary>
public static class AdminCreatePromotionLevelMetaField
{
    public static readonly RouteMetadata AdminCreatePromotionLevel = new(
        "AdminCreatePromotionLevel",
        "Create a new promotion level",
        """
            Creates a new homepage placement upgrade option (e.g. "Featured — 7 days").
            \n
            This endpoint creates a promotion level by:\n
            - Validating the name, duration, and price\n
            - Checking that no promotion level with the same name already exists\n
            - Creating the level with active status\n
            - Returning the created promotion level details\n
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Request Body:**\n
            - name: The unique display name for the promotion level (max 40 characters)\n
            - durationDays: The homepage placement duration in days (must be > 0)\n
            - priceUsd: The price in USD (must be >= 0)\n
            \n
            **Response Codes:**\n
            - Returns 201 Created with promotion level details on success\n
            - Returns 400 Bad Request if validation fails\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 409 Conflict if promotion level name already exists\n
        """
    );
}
