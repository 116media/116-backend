using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.UnbookmarkShortVideo;

/// <summary>
/// Contains metadata information for the unbookmark short video route.
/// </summary>
public static class PublicUnbookmarkShortVideoMetaField
{
    /// <summary>
    /// Route metadata for the PublicUnbookmarkShortVideo endpoint.
    /// </summary>
    public static readonly RouteMetadata PublicUnbookmarkShortVideo = new(
        "PublicUnbookmarkShortVideo",
        "Remove a bookmark from a short video",
        """
            Removes a bookmark from a short video for the authenticated user.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have an active account\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on success\n
            - Returns 400 Bad Request if the short video has not been bookmarked\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 404 Not Found if the short video does not exist\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
