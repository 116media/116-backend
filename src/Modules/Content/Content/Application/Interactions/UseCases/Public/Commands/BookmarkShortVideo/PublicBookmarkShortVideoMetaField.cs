using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.BookmarkShortVideo;

/// <summary>
/// Contains metadata information for the bookmark short video route.
/// </summary>
public static class PublicBookmarkShortVideoMetaField
{
    /// <summary>
    /// Route metadata for the PublicBookmarkShortVideo endpoint.
    /// </summary>
    public static readonly RouteMetadata PublicBookmarkShortVideo = new(
        "PublicBookmarkShortVideo",
        "Bookmark a short video",
        """
            Records that the authenticated user has bookmarked a short video.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have an active account\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 404 Not Found if the short video does not exist\n
            - Returns 409 Conflict if the user has already bookmarked this short video\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
