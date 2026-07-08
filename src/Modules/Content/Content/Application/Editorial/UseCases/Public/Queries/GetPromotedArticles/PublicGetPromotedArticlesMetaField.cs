using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetPromotedArticles;

/// <summary>
/// Contains metadata information for the get promoted articles route.
/// </summary>
public static class PublicGetPromotedArticlesMetaField
{
    public static readonly RouteMetadata GetPromotedArticles = new(
        "GetPromotedArticles",
        "List promoted articles",
        """
            Retrieves the list of currently promoted published articles for public consumption.
            \n
            Promoted articles are published articles with an active paid promotion on the homepage
            or highlighted sections of the site. Only published articles marked as promoted
            are returned by this endpoint.
            \n
            **Authentication Requirements:**\n
            - No authentication required\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with the list of promoted articles\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
