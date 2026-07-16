using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Interactions.UseCases.Public.Queries.GetOwnRatedVideos;

/// <summary>
/// Route metadata for the current-user rated-video collection.
/// </summary>
public static class PublicGetOwnRatedVideosMetaField
{
    public static readonly RouteMetadata GetOwnRatedVideos = new(
        "PublicGetOwnRatedVideos",
        "Get my rated videos",
        """
            Returns a paginated list of published videos the authenticated user has rated, exposing
            the user's own star rating, newest interaction first.
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
