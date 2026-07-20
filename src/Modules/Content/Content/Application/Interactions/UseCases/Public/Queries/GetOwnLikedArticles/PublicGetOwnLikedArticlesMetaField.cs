using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Interactions.UseCases.Public.Queries.GetOwnLikedArticles;

/// <summary>
/// Contains metadata information for the get my liked articles route.
/// </summary>
public static class PublicGetOwnLikedArticlesMetaField
{
    public static readonly RouteMetadata GetOwnLikedArticles = new(
        "PublicGetOwnLikedArticles",
        "Get my liked articles",
        """
            Returns a paginated list of published articles currently liked by the authenticated user,
            newest like first.
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
