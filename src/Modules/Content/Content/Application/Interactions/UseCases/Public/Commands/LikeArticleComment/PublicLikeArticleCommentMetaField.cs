using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.LikeArticleComment;

/// <summary>
/// Contains metadata information for the like article comment route.
/// </summary>
public static class PublicLikeArticleCommentMetaField
{
    public static readonly RouteMetadata PublicLikeArticleComment = new(
        "PublicLikeArticleComment",
        "Like an article comment",
        """
            Records that the authenticated user has liked a comment. Idempotent — liking a
            comment that is already liked has no effect and still returns success.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have an active account\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 404 Not Found if the comment does not exist\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
