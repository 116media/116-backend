using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.BookmarkArticle;

/// <summary>
/// Contains metadata information for the bookmark article route.
/// </summary>
public static class PublicBookmarkArticleMetaField
{
    public static readonly RouteMetadata PublicBookmarkArticle = new(
        "PublicBookmarkArticle",
        "Bookmark an article",
        """
            Records that the authenticated user has bookmarked an article for later reading.
            \n
            Returns 409 Conflict if the user has already bookmarked the article.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have an active account\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 404 Not Found if the article does not exist\n
            - Returns 409 Conflict if the user has already bookmarked this article\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
