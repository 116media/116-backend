using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.RecordShortVideoView;

/// <summary>
/// Contains metadata information for the record short video view route.
/// </summary>
public static class PublicRecordShortVideoViewMetaField
{
    public static readonly RouteMetadata PublicRecordShortVideoView = new(
        "PublicRecordShortVideoView",
        "Record a view for a short video",
        """
            Records a view event for a short video, incrementing its view count.
            This endpoint is publicly accessible and does not require authentication.
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
