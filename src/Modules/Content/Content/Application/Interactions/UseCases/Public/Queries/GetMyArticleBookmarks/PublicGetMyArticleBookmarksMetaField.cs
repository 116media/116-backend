using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Interactions.UseCases.Public.Queries.GetMyArticleBookmarks;

/// <summary>
/// Contains metadata information for the get my article bookmarks route.
/// </summary>
public static class PublicGetMyArticleBookmarksMetaField
{
    public static readonly RouteMetadata PublicGetMyArticleBookmarks = new(
        "PublicGetMyArticleBookmarks",
        "Get my bookmarked articles",
        """
            Returns a paginated list of articles bookmarked by the authenticated user.
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
