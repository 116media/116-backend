using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Lookup.UseCases.Public.Queries.GetActivePromotionLevels;

/// <summary>
/// Contains metadata information for the public get active promotion levels route.
/// </summary>
public static class PublicGetActivePromotionLevelsMetaField
{
    public static readonly RouteMetadata PublicGetActivePromotionLevels = new(
        "PublicGetActivePromotionLevels",
        "Get active promotion levels",
        """
            Returns all currently active promotion levels for content discovery and purchasing decisions.
            \n
            This endpoint is publicly accessible and does not require authentication.
            \n
            **Response Codes:**\n
            - Returns 200 OK with the list of active promotion levels on success\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
