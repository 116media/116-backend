using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.ShareVideo;

/// <summary>
/// Contains metadata information for the share video route.
/// </summary>
public static class PublicShareVideoMetaField
{
    public static readonly RouteMetadata PublicShareVideo = new(
        "PublicShareVideo",
        "Record a video share",
        """
            Records a share event for a video. Works for both authenticated users and anonymous visitors.
            \n
            **Authentication Requirements:**\n
            - No authentication required — anonymous access is permitted\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on success\n
            - Returns 404 Not Found if the video does not exist\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
