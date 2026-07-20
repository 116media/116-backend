using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Interactions.UseCases.Public.Queries.GetOwnCommentedArticles;

/// <summary>
/// Contains metadata information for the get my commented articles route.
/// </summary>
public static class PublicGetOwnCommentedArticlesMetaField
{
    public static readonly RouteMetadata GetOwnCommentedArticles = new(
        "PublicGetOwnCommentedArticles",
        "Get my commented articles",
        """
            Returns a paginated list of published articles the authenticated user has commented on,
            grouped per article with the latest comment and total comment count, newest activity first.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have an active account\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
