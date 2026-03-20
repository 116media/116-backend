using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.ShareShortVideo;

/// <summary>
/// Contains metadata information for the share short video route.
/// </summary>
public static class PublicShareShortVideoMetaField
{
    public static readonly RouteMetadata PublicShareShortVideo = new(
        "PublicShareShortVideo",
        "Share a short video",
        """
            Records a share event for a short video. This endpoint is publicly accessible
            and does not require authentication.
            \n
            **Authentication Requirements:**\n
            - No authentication required (anonymous access allowed)\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on success\n
            - Returns 404 Not Found if the short video does not exist\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
