using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Interactions.UseCases.Public.Queries.GetOwnCommentsForArticle;

/// <summary>
/// Contains metadata information for the get my comments for an article route.
/// </summary>
public static class PublicGetOwnCommentsForArticleMetaField
{
    public static readonly RouteMetadata GetOwnCommentsForArticle = new(
        "PublicGetOwnCommentsForArticle",
        "Get my comments for an article",
        """
            Returns a paginated list of the authenticated user's own non-deleted comments on a
            specific published article, newest first.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have an active account\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 404 Not Found if the article does not exist or is not published\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
