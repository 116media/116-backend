using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetPopularArticles;

/// <summary>
/// Contains metadata information for the public get popular articles route.
/// </summary>
public static class PublicGetPopularArticlesMetaField
{
    public static readonly RouteMetadata GetPopularArticles = new(
        "PublicGetPopularArticles",
        "Get popular articles",
        """
            Returns published articles ranked by a weighted engagement score
            (likes, comments, shares, bookmarks), tie-broken by publish date descending.
            \n
            Results are cached server-side for 10 minutes to avoid running the
            ranking query on every request.
            \n
            **Query Parameters:**\n
            - `limit` (optional, default 10, max 50): maximum number of articles to return\n
            - `categoryId` (optional): rank only articles in this category\n
            - `excludeId` (optional): article id to omit, e.g. the article currently being viewed\n
            \n
            This endpoint is publicly accessible and does not require authentication.
            \n
            **Response Codes:**\n
            - Returns 200 OK with the list of popular articles on success\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
