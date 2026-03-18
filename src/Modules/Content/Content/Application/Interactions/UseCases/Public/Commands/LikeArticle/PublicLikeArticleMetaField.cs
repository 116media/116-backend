using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.LikeArticle;

/// <summary>
/// Contains metadata information for the like article route.
/// </summary>
public static class PublicLikeArticleMetaField
{
    public static readonly RouteMetadata PublicLikeArticle = new(
        "PublicLikeArticle",
        "Like an article",
        """
            Records that the authenticated user has liked an article.
            \n
            Returns 409 Conflict if the user has already liked the article.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have an active account\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 404 Not Found if the article does not exist\n
            - Returns 409 Conflict if the user has already liked this article\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
