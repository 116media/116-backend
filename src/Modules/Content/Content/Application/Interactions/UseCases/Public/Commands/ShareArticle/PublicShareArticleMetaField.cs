using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.ShareArticle;

/// <summary>
/// Contains metadata information for the share article route.
/// </summary>
public static class PublicShareArticleMetaField
{
    public static readonly RouteMetadata PublicShareArticle = new(
        "PublicShareArticle",
        "Record an article share",
        """
            Records a share event for an article. Works for both authenticated users and anonymous visitors.
            \n
            **Authentication Requirements:**\n
            - No authentication required — anonymous access is permitted\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on success\n
            - Returns 404 Not Found if the article does not exist\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
