using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetArtistArticles;

/// <summary>
/// Contains metadata information for the get artist articles route.
/// </summary>
public static class PublicGetArtistArticlesMetaField
{
    public static readonly RouteMetadata GetArtistArticles = new(
        "GetArtistArticles",
        "Get published articles tagged to an artist by slug",
        """
            Retrieves a paginated page of published articles tagged to the artist, newest
            first. Draft and archived articles never appear.
            \n
            The artist is addressed by URL-safe slug; the lookup is case-insensitive.
            \n
            **Authentication Requirements:**\n
            - No authentication required (public endpoint)\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with the paginated articles on success\n
            - Returns 404 Not Found if no artist profile matches the given slug\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
