using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Public.Commands.VoteOnLyricsRevision;

/// <summary>
/// Contains metadata information for the vote on lyrics revision route.
/// </summary>
public static class PublicVoteOnLyricsRevisionMetaField
{
    public static readonly RouteMetadata VoteOnLyricsRevision = new(
        "VoteOnLyricsRevision",
        "Vote on a proposed lyrics correction",
        """
            Casts a community vote — approve or reject — on a pending lyrics-text correction
            revision.
            \n
            A revision that reaches the net approval threshold is automatically accepted and
            its proposed text is applied to the lyrics page in the same operation as this vote.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have an active account\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 404 Not Found if the revision does not exist\n
            - Returns 409 Conflict if the user has already voted on this revision\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
