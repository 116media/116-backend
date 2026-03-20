using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.AddArticleComment;

/// <summary>
/// Contains metadata information for the add article comment route.
/// </summary>
public static class PublicAddArticleCommentMetaField
{
    public static readonly RouteMetadata PublicAddArticleComment = new(
        "PublicAddArticleComment",
        "Post a comment on an article",
        """
            Posts a new comment on an article. The comment body must not exceed 1000 characters.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have an active account\n
            \n
            **Response Codes:**\n
            - Returns 201 Created on success with the comment DTO\n
            - Returns 400 Bad Request if the body exceeds the maximum length\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 404 Not Found if the article does not exist\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
