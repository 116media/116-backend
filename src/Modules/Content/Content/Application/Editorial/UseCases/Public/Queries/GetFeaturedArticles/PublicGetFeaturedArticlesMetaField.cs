using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetFeaturedArticles;

/// <summary>
/// Contains metadata information for the get featured articles route.
/// </summary>
public static class PublicGetFeaturedArticlesMetaField
{
    public static readonly RouteMetadata PublicGetFeaturedArticles = new(
        "GetFeaturedArticles",
        "List featured articles",
        """
            Retrieves the list of currently featured published articles for public consumption.
            \n
            Featured articles are handpicked published articles promoted on the homepage
            or highlighted sections of the site. Only published articles marked as featured
            are returned by this endpoint.
            \n
            **Authentication Requirements:**\n
            - No authentication required\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with the list of featured articles\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
