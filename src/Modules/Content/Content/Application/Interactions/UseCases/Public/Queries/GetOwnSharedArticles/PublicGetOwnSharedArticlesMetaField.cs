using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Interactions.UseCases.Public.Queries.GetOwnSharedArticles;

/// <summary>
/// Contains metadata information for the get my shared articles route.
/// </summary>
public static class PublicGetOwnSharedArticlesMetaField
{
    public static readonly RouteMetadata GetOwnSharedArticles = new(
        "PublicGetOwnSharedArticles",
        "Get my shared articles",
        """
            Returns a paginated list of published articles the authenticated user has shared,
            grouped per article with the user's own share count and latest share channel, newest first.
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
