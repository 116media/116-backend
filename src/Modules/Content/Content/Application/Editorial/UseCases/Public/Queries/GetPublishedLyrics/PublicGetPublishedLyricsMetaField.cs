using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublishedLyrics;

/// <summary>
/// Contains metadata information for the get published lyrics route.
/// </summary>
public static class PublicGetPublishedLyricsMetaField
{
    public static readonly RouteMetadata GetPublishedLyrics = new(
        "GetPublishedLyrics",
        "List published lyrics pages",
        """
            Retrieves a paginated list of all published lyrics pages for public consumption.
            \n
            Supports optional filtering by search term, language, and category. Results are
            returned as a paginated list with summary information suitable for lyrics feed and
            browsing views.
            \n
            **Authentication Requirements:**\n
            - No authentication required\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with paginated lyrics list on success\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
