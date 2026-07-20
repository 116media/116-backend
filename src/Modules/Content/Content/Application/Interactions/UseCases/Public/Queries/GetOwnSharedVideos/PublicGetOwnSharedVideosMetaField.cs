using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Interactions.UseCases.Public.Queries.GetOwnSharedVideos;

/// <summary>
/// Route metadata for the current-user shared-video collection.
/// </summary>
public static class PublicGetOwnSharedVideosMetaField
{
    public static readonly RouteMetadata GetOwnSharedVideos = new(
        "PublicGetOwnSharedVideos",
        "Get my shared videos",
        """
            Returns a paginated list of published videos the authenticated user has shared, grouped
            per video with the user's own share count and latest share channel, newest share first.
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
