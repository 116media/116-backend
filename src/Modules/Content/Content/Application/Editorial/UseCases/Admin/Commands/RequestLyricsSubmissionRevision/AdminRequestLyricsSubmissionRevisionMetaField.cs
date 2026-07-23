using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.RequestLyricsSubmissionRevision;

/// <summary>
/// Contains metadata information for the admin request lyrics submission revision route.
/// </summary>
public static class AdminRequestLyricsSubmissionRevisionMetaField
{
    public static readonly RouteMetadata RequestLyricsSubmissionRevision = new(
        "RequestLyricsSubmissionRevision",
        "Request changes to a community lyrics submission",
        """
            Asks the submitter to revise and resubmit their pending community lyrics
            submission, with a mandatory note describing the requested changes. The submission
            is neither approved nor rejected outright.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on success\n
            - Returns 400 Bad Request if the note is missing\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
            - Returns 404 Not Found if the submission does not exist\n
            - Returns 409 Conflict if the submission is no longer pending\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
