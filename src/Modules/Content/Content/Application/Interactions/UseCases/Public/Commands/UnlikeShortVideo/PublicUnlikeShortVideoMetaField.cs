using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.UnlikeShortVideo;

/// <summary>
/// Contains metadata information for the unlike short video route.
/// </summary>
public static class PublicUnlikeShortVideoMetaField
{
    /// <summary>
    /// Route metadata for the PublicUnlikeShortVideo endpoint.
    /// </summary>
    public static readonly RouteMetadata PublicUnlikeShortVideo = new(
        "PublicUnlikeShortVideo",
        "Remove a like from a short video",
        """
            Removes the authenticated user's like from a short video.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have an active account\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on success\n
            - Returns 400 Bad Request if the user has not liked this short video\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 404 Not Found if the short video does not exist\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
