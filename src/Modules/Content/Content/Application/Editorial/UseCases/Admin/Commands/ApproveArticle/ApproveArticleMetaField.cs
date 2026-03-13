using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ApproveArticle;

/// <summary>
/// Contains metadata information for the approve article route.
/// </summary>
public static class ApproveArticleMetaField
{
    public static readonly RouteMetadata ApproveArticle = new(
        "ApproveArticle",
        "Approve an article for publication",
        """
            Approves an article that is currently in <c>PendingReview</c> status,
            transitioning it to <c>Approved</c> and clearing it for publication.
            \n
            Only articles in <c>PendingReview</c> status can be approved.
            Attempting to approve an article in any other status will return a 400 Bad Request.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 204 No Content on success\n
            - Returns 400 Bad Request if the article is not in PendingReview status\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 404 Not Found if the article does not exist\n
        """
    );
}
