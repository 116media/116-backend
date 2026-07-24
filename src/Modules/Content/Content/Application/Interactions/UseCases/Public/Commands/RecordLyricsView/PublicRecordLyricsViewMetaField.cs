using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.RecordLyricsView;

/// <summary>
/// Contains metadata information for the record lyrics view route.
/// </summary>
public static class PublicRecordLyricsViewMetaField
{
    public static readonly RouteMetadata RecordLyricsView = new(
        "PublicRecordLyricsView",
        "Record a lyrics page view",
        """
            Records a view event for a lyrics page, gated by the read-time view-counting
            algorithm: the reported dwell time and scroll depth are checked against the
            server-computed expected reading time before the displayed count increments.
            \n
            **Authentication Requirements:**\n
            - No authentication required (public endpoint)\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on success, with <c>isCounted</c> indicating whether the view
              incremented the displayed count\n
            - Returns 404 Not Found if the lyrics page does not exist\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
