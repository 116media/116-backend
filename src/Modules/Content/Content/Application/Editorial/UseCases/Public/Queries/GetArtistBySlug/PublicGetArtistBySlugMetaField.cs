using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetArtistBySlug;

/// <summary>
/// Contains metadata information for the get artist by slug route.
/// </summary>
public static class PublicGetArtistBySlugMetaField
{
    public static readonly RouteMetadata GetArtistBySlug = new(
        "GetArtistBySlug",
        "Get an artist's public profile page by slug",
        """
            Retrieves an artist's public profile page using its URL-safe slug, including
            paginated published lyrics and videos linked to the artist. Only Published
            content is returned.
            \n
            The lookup is case-insensitive.
            \n
            **Authentication Requirements:**\n
            - No authentication required (public endpoint)\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with the artist profile and paginated catalog on success\n
            - Returns 404 Not Found if no artist profile matches the given slug\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
