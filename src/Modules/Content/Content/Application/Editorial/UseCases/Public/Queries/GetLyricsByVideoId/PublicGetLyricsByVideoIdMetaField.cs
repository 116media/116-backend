using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsByVideoId;

/// <summary>
/// Contains metadata information for the get lyrics by video ID route.
/// </summary>
public static class PublicGetLyricsByVideoIdMetaField
{
    public static readonly RouteMetadata GetLyricsByVideoId = new(
        "GetLyricsByVideoId",
        "Get lyrics linked to a video",
        """
            Retrieves the lyrics page associated with a given video ID.
            \n
            Returns the full lyrics details if a lyrics page is linked to the video.
            \n
            **Authentication Requirements:**\n
            - No authentication required (public endpoint)\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with lyrics details on success\n
            - Returns 404 Not Found if no lyrics are linked to the given video\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
