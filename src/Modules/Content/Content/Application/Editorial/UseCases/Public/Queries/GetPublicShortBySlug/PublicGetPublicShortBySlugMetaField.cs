using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublicShortBySlug;

/// <summary>
/// Contains metadata information for the get public short video by slug route.
/// </summary>
public static class PublicGetPublicShortBySlugMetaField
{
    public static readonly RouteMetadata PublicGetPublicShortBySlug = new(
        "GetPublicShortBySlug",
        "Get active short video by slug",
        """
            Retrieves the full details of a single active short video clip by its URL slug.
            \n
            Powers the individual short video page with the video player and engagement counters.
            Returns 404 if the short video does not exist or is not active.
            \n
            **Authentication Requirements:**\n
            - No authentication required\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with short video details on success\n
            - Returns 404 Not Found if the short video does not exist or is not active\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
