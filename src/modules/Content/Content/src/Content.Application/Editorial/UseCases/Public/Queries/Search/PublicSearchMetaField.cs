using _116.BuildingBlocks.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.Search;

/// <summary>
/// Route metadata for the public search endpoint.
/// </summary>
public static class PublicSearchMetaField
{
    /// <summary>
    /// The public search route.
    /// </summary>
    public static readonly RouteMetadata Search = new(
        "PublicSearch",
        "Search across the published content",
        """
            Searches the published articles and videos with PostgreSQL full-text search,
            ranked by relevance. Accent-insensitive and stemmed for French. Returns the
            hits for the requested scope plus the hit counts for every scope, so a client
            can label its scope tabs from one response.
            \n
            **Response Codes:**\n
            - Returns 200 OK with the ranked hits, or an empty list when nothing matches\n
            - Returns 400 Bad Request when the term is shorter than 2 characters\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
