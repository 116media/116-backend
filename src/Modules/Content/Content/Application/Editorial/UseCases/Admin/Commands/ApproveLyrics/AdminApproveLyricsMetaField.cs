using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ApproveLyrics;

/// <summary>
/// Contains metadata information for the approve lyrics route.
/// </summary>
public static class AdminApproveLyricsMetaField
{
    public static readonly RouteMetadata ApproveLyrics = new(
        "ApproveLyrics",
        "Approve a lyrics page for publication",
        """
            Approves a lyrics page that is currently in <c>PendingReview</c> status,
            transitioning it to <c>Approved</c> and clearing it for publication.
            \n
            Only lyrics pages in <c>PendingReview</c> status can be approved.
            Attempting to approve a lyrics page in any other status will return a 400 Bad Request.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on success\n
            - Returns 400 Bad Request if the lyrics page is not in PendingReview status\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 404 Not Found if the lyrics page does not exist\n
        """
    );
}
