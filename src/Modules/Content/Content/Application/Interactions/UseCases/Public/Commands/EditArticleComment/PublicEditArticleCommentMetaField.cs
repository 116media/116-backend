using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.EditArticleComment;

/// <summary>
/// Contains metadata information for the edit article comment route.
/// </summary>
public static class PublicEditArticleCommentMetaField
{
    public static readonly RouteMetadata PublicEditArticleComment = new(
        "PublicEditArticleComment",
        "Edit an article comment",
        """
            Updates the body of an existing article comment. Only the comment owner can edit their own comment.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have an active account\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on success\n
            - Returns 400 Bad Request if the user is not the comment owner or body is invalid\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 404 Not Found if the comment does not exist\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
