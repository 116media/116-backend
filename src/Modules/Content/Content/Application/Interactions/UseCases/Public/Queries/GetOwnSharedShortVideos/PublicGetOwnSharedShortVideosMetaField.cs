using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Interactions.UseCases.Public.Queries.GetOwnSharedShortVideos;

/// <summary>
/// Metadata for the get my shared short videos route.
/// </summary>
public static class PublicGetOwnSharedShortVideosMetaField
{
    public static readonly RouteMetadata GetOwnSharedShortVideos = new(
        "PublicGetOwnSharedShortVideos",
        "Get my shared short videos",
        """
            Returns a paginated list of active short videos shared by the authenticated user,
            grouped per short video with the user's own share count, newest share first.
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
