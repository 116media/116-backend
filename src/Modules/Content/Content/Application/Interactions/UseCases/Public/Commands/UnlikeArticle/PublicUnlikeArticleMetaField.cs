using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.UnlikeArticle;

/// <summary>
/// Contains metadata information for the unlike article route.
/// </summary>
public static class PublicUnlikeArticleMetaField
{
    public static readonly RouteMetadata PublicUnlikeArticle = new(
        "PublicUnlikeArticle",
        "Remove a like from an article",
        """
            Removes the authenticated user's like from an article.
            \n
            Returns 400 Bad Request if the user has not liked the article.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have an active account\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on success\n
            - Returns 400 Bad Request if the user has not liked this article\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 404 Not Found if the article does not exist\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
