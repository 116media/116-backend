using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Interactions.UseCases.Admin.Commands.DeleteArticleComment;

/// <summary>
/// Contains metadata information for the admin delete article comment route.
/// </summary>
public static class AdminDeleteArticleCommentMetaField
{
    public static readonly RouteMetadata AdminDeleteArticleComment = new(
        "AdminDeleteArticleComment",
        "Delete any article comment",
        """
            Soft-deletes any article comment regardless of ownership.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin or SuperAdmin role\n
            - Returns 404 Not Found if the comment does not exist\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
