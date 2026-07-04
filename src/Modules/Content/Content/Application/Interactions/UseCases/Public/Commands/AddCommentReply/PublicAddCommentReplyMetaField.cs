using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.AddCommentReply;

/// <summary>
/// Contains metadata information for the add comment reply route.
/// </summary>
public static class PublicAddCommentReplyMetaField
{
    public static readonly RouteMetadata AddCommentReply = new(
        "PublicAddCommentReply",
        "Reply to an article comment",
        """
            Posts a single-level reply to an existing top-level comment on an article.
            \n
            Replies to replies are rejected — only one level of threading is supported.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have an active account\n
            \n
            **Response Codes:**\n
            - Returns 201 Created on success\n
            - Returns 400 Bad Request if the body is invalid or the parent is itself a reply\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 404 Not Found if the article or parent comment does not exist\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
