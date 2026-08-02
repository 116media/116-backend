using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.DecideLyricsRevision;

/// <summary>
/// Contains metadata information for the admin decide lyrics revision route.
/// </summary>
public static class AdminDecideLyricsRevisionMetaField
{
    public static readonly RouteMetadata DecideLyricsRevision = new(
        "DecideLyricsRevision",
        "Moderator decision on a lyrics correction",
        """
            Lets a moderator accept or reject a pending lyrics-text correction revision
            directly, bypassing the community vote tally. Accepting applies the revision's
            proposed text to the lyrics page in the same operation as the decision.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
            - Returns 404 Not Found if the revision does not exist\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
