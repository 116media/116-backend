using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Interactions.UseCases.Public.Queries.GetOwnLikedShortVideos;

/// <summary>
/// Metadata for the get my liked short videos route.
/// </summary>
public static class PublicGetOwnLikedShortVideosMetaField
{
    public static readonly RouteMetadata GetOwnLikedShortVideos = new(
        "PublicGetOwnLikedShortVideos",
        "Get my liked short videos",
        """
            Returns a paginated list of active short videos currently liked by the authenticated
            user, newest like first.
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
