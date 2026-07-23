using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Public.Commands.SubmitLyrics;

/// <summary>
/// Contains metadata information for the submit lyrics route.
/// </summary>
public static class PublicSubmitLyricsMetaField
{
    public static readonly RouteMetadata SubmitLyrics = new(
        "CreateLyricsSubmission",
        "Submit a new song",
        """
            Submits a new song to the platform.
            \n
            A submitter who owns a claimed artist profile has their song created directly as a
            real lyrics record, in the `Draft` status — no auto-publish, it still goes through
            the normal editorial workflow — skipping the moderation queue entirely. This gate is
            identity-based (the submitter's own user id), never based on the artist name text
            sent in the request.
            \n
            Anyone else's submission enters the community moderation queue and requires an
            artist name, since there is no claimed profile to fall back on.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have an active account\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on success\n
            - Returns 400 Bad Request if a required field is missing\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 409 Conflict if the slug is already taken (verified-artist fast path only)\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
