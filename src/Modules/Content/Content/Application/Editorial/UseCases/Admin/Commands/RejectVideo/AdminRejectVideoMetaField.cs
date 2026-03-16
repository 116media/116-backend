using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.RejectVideo;

/// <summary>
/// Contains metadata information for the reject video route.
/// </summary>
public static class AdminRejectVideoMetaField
{
    public static readonly RouteMetadata AdminRejectVideo = new(
        "RejectVideo",
        "Reject a video during editorial review",
        """
            Rejects a video that is currently in <c>PendingReview</c> status,
            transitioning it to <c>Rejected</c> with a mandatory rejection reason.
            \n
            Only videos in <c>PendingReview</c> status can be rejected.
            Attempting to reject a video in any other status will return a 400 Bad Request.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 204 No Content on success\n
            - Returns 400 Bad Request if the video is not in PendingReview status\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 404 Not Found if the video does not exist\n
        """
    );
}
