using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetVideoBySlug;

/// <summary>
/// Contains metadata information for the get video by slug route.
/// </summary>
public static class GetVideoBySlugMetaField
{
    public static readonly RouteMetadata GetVideoBySlug = new(
        "GetVideoBySlug",
        "Get published video by slug",
        """
            Retrieves the full details of a single published video by its URL slug.
            \n
            Powers the individual video page with the embedded YouTube player, star ratings,
            and related videos. Returns 404 if the video does not exist or is not published.
            \n
            **Authentication Requirements:**\n
            - No authentication required\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with video detail on success\n
            - Returns 404 Not Found if video does not exist or is not published\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
