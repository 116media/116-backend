using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetArticlePromotionFeed;

/// <summary>
/// Contains metadata information for the article promotion feed route.
/// </summary>
public static class PublicGetArticlePromotionFeedMetaField
{
    public static readonly RouteMetadata GetArticlePromotionFeed = new(
        "GetArticlePromotionFeed",
        "Article homepage promotion feed",
        """
            Returns the homepage article promotion feed, grouping promoted published articles by
            spot priority (1, 2, 3).
            \n
            Each spot maps to a visual region on the homepage grid. Spot 3 distributes articles
            across two columns (a and b). Empty spots are filled with gossip fallback articles
            from the category flagged as the gossip fallback source.
            \n
            A gossip strip of up to 3 additional articles is included below the grid.
            \n
            **Authentication Requirements:**\n
            - No authentication required\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with the full promotion feed\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
