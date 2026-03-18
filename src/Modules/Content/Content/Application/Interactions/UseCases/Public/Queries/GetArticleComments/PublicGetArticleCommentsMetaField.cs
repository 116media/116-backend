using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Interactions.UseCases.Public.Queries.GetArticleComments;

/// <summary>
/// Contains metadata information for the get article comments route.
/// </summary>
public static class PublicGetArticleCommentsMetaField
{
    public static readonly RouteMetadata PublicGetArticleComments = new(
        "PublicGetArticleComments",
        "List comments for an article",
        """
            Returns a paginated list of comments for a given article.
            Soft-deleted comments are included but their body is returned as null.
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
