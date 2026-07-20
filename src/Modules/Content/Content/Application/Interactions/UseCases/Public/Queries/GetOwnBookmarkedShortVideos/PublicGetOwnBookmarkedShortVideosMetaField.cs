using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Interactions.UseCases.Public.Queries.GetOwnBookmarkedShortVideos;

/// <summary>
/// Metadata for the get my bookmarked short videos route.
/// </summary>
public static class PublicGetOwnBookmarkedShortVideosMetaField
{
    public static readonly RouteMetadata GetOwnBookmarkedShortVideos = new(
        "PublicGetOwnBookmarkedShortVideos",
        "Get my bookmarked short videos",
        """
            Returns a paginated list of active short videos currently bookmarked by the authenticated
            user, newest bookmark first.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have an active account\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
