using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.SubmitLyrics;

/// <summary>
/// Contains metadata information for the submit lyrics route.
/// </summary>
public static class AdminSubmitLyricsMetaField
{
    public static readonly RouteMetadata SubmitLyrics = new(
        "SubmitLyrics",
        "Submit a lyrics page for review",
        """
            Submits a lyrics page for editorial review or payment processing.
            \n
            Free lyrics pages transition directly to <c>PendingReview</c> status, where the
            editorial team can approve or reject them.
            Paid (commissioned) lyrics pages transition to <c>PendingPayment</c> status, awaiting
            customer payment verification before entering the editorial review queue.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 404 Not Found if the lyrics page does not exist\n
        """
    );
}
