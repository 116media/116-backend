using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsBySlug;

/// <summary>
/// Contains metadata information for the get lyrics by slug route.
/// </summary>
public static class PublicGetLyricsBySlugMetaField
{
    public static readonly RouteMetadata GetLyricsBySlug = new(
        "GetLyricsBySlug",
        "Get lyrics by slug",
        """
            Retrieves a lyrics page using its URL-safe slug as a path parameter.
            This endpoint is designed for SEO-friendly public access to lyrics pages
            (e.g., <c>/api/v1/public/lyrics/fally-ipupa-eloko-oyo-lyrics</c>).
            \n
            The lookup is case-insensitive.
            \n
            **Authentication Requirements:**\n
            - No authentication required (public endpoint)\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with lyrics details on success\n
            - Returns 404 Not Found if no lyrics match the given slug\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
