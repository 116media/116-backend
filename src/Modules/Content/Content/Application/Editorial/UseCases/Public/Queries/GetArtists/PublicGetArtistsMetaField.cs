using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetArtists;

/// <summary>
/// Contains metadata information for the public artist directory route.
/// </summary>
public static class PublicGetArtistsMetaField
{
    public static readonly RouteMetadata GetArtists = new(
        "GetArtists",
        "List artists for the public directory",
        """
            Retrieves the public artist directory: only artists with at least one published
            item on any profile surface, ordered by accent-folded name, each carrying its
            total item count. Includes the distinct available initial letters so the A-Z
            rail can disable empty buckets.
            \n
            Supports an initial-letter filter (A-Z or #) or an accent-insensitive name
            search of at least two characters — never both at once.
            \n
            **Authentication Requirements:**\n
            - No authentication required (public endpoint)\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with the directory page and available letters on success\n
            - Returns 400 Bad Request if letter and search are combined or the search is too short\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
