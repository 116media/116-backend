using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ApproveLyricsSubmission;

/// <summary>
/// Contains metadata information for the admin approve lyrics submission route.
/// </summary>
public static class AdminApproveLyricsSubmissionMetaField
{
    public static readonly RouteMetadata ApproveLyricsSubmission = new(
        "ApproveLyricsSubmission",
        "Approve a community lyrics submission",
        """
            Approves a pending community lyrics submission, promoting it into a real lyrics
            record filed under the default free lyrics category. The new record starts in
            `Draft` and goes through the normal spec-01 editorial workflow like any other lyrics
            record — no separate publish concept exists for community-originated content.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on success\n
            - Returns 400 Bad Request if the slug is missing or malformed\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
            - Returns 404 Not Found if the submission does not exist\n
            - Returns 409 Conflict if the submission is no longer pending, or if the slug is already taken\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
