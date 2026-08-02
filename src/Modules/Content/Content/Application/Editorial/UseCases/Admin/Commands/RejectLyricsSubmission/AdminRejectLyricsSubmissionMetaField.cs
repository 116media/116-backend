using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.RejectLyricsSubmission;

/// <summary>
/// Contains metadata information for the admin reject lyrics submission route.
/// </summary>
public static class AdminRejectLyricsSubmissionMetaField
{
    public static readonly RouteMetadata RejectLyricsSubmission = new(
        "RejectLyricsSubmission",
        "Reject a community lyrics submission",
        """
            Rejects a pending community lyrics submission outright, with a mandatory note
            visible to the submitter. The submission never becomes a lyrics record.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on success\n
            - Returns 400 Bad Request if the rejection note is missing\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
            - Returns 404 Not Found if the submission does not exist\n
            - Returns 409 Conflict if the submission is no longer pending\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
