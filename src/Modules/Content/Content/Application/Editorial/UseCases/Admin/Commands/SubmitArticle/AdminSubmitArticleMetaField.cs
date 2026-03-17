using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.SubmitArticle;

/// <summary>
/// Contains metadata information for the submit article route.
/// </summary>
public static class AdminSubmitArticleMetaField
{
    public static readonly RouteMetadata AdminSubmitArticle = new(
        "SubmitArticle",
        "Submit article for review",
        """
            Submits an article for editorial review or payment processing.
            \n
            Free articles transition directly to <c>PendingReview</c> status, where the
            editorial team can approve or reject them.
            Paid (commissioned) articles transition to <c>PendingPayment</c> status, awaiting
            customer payment verification before entering the editorial review queue.
            \n
            This is step 3 of the article workflow, following draft creation (step 1) and
            content editing (step 2).
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 404 Not Found if the article does not exist\n
        """
    );
}
