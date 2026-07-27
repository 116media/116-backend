using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetArtistReleases;

/// <summary>
/// Contains metadata information for the get artist releases route.
/// </summary>
public static class PublicGetArtistReleasesMetaField
{
    public static readonly RouteMetadata GetArtistReleases = new(
        "GetArtistReleases",
        "Get an artist's releases of a given type by slug",
        """
            Retrieves a paginated page of an artist's releases filtered by release type
            (Album, Mixtape, EP or Single), newest first with unknown years last.
            \n
            The artist is addressed by URL-safe slug; the lookup is case-insensitive.
            \n
            **Authentication Requirements:**\n
            - No authentication required (public endpoint)\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with the paginated releases on success\n
            - Returns 404 Not Found if no artist profile matches the given slug\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
