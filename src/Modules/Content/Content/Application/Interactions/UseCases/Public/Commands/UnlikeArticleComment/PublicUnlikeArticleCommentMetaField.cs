using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.UnlikeArticleComment;

/// <summary>
/// Contains metadata information for the unlike article comment route.
/// </summary>
public static class PublicUnlikeArticleCommentMetaField
{
    public static readonly RouteMetadata PublicUnlikeArticleComment = new(
        "PublicUnlikeArticleComment",
        "Unlike an article comment",
        """
            Removes the authenticated user's like from a comment. Idempotent — unliking a
            comment that is not liked has no effect and still returns success.
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
