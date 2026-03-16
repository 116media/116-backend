using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublishedVideos;

/// <summary>
/// Contains metadata information for the get published videos route.
/// </summary>
public static class PublicGetPublishedVideosMetaField
{
    public static readonly RouteMetadata PublicGetPublishedVideos = new(
        "GetPublishedVideos",
        "List published videos",
        """
            Retrieves a paginated list of all published videos for public consumption.
            \n
            Supports optional filtering by category. Results are returned as a paginated list
            with summary information suitable for video feed and browsing views.
            \n
            **Authentication Requirements:**\n
            - No authentication required\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with paginated video list on success\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
