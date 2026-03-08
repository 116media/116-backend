using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Lookup.UseCases.Admin.Queries.GetAllPromotionLevels;

/// <summary>
/// Contains metadata information for the get all promotion levels route.
/// </summary>
public static class GetAllPromotionLevelsMetaField
{
    public static readonly RouteMetadata GetAllPromotionLevels = new(
        "GetAllPromotionLevels",
        "List all promotion levels",
        """
            Returns the complete list of promotion levels available for order upsells.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with the list of promotion levels on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
        """
    );
}
