using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.RejectArticle;

/// <summary>
/// Contains metadata information for the reject article route.
/// </summary>
public static class AdminRejectArticleMetaField
{
    public static readonly RouteMetadata AdminRejectArticle = new(
        "RejectArticle",
        "Reject an article during editorial review",
        """
            Rejects an article that is currently in <c>PendingReview</c> status,
            transitioning it to <c>Rejected</c> with a mandatory reason.
            \n
            The rejection reason is stored on the article and is visible to the editorial team.
            The admin can revise the article content and resubmit it for review.
            \n
            Only articles in <c>PendingReview</c> status can be rejected.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on success\n
            - Returns 400 Bad Request if the article is not in PendingReview status or reason is missing\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 404 Not Found if the article does not exist\n
        """
    );
}
