using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublishedArticles;

/// <summary>
/// Contains metadata information for the get published articles route.
/// </summary>
public static class PublicGetPublishedArticlesMetaField
{
    public static readonly RouteMetadata PublicGetPublishedArticles = new(
        "GetPublishedArticles",
        "List published articles",
        """
            Retrieves a paginated list of all published articles for public consumption.
            \n
            Supports optional filtering by category. Results are returned as a paginated list
            with summary information suitable for article feed and browsing views.
            \n
            **Authentication Requirements:**\n
            - No authentication required\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with paginated article list on success\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
