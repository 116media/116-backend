using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Public.Commands.VoteOnTranslationRevision;

/// <summary>
/// Contains metadata information for the vote on translation revision route.
/// </summary>
public static class PublicVoteOnTranslationRevisionMetaField
{
    public static readonly RouteMetadata VoteOnTranslationRevision = new(
        "VoteOnTranslationRevision",
        "Vote on a pending translation revision",
        """
            Casts the authenticated user's vote — approve or reject — on a pending translation
            revision.
            \n
            A user may vote at most once per revision. Once the revision's net approvals
            (approvals minus rejections) reach the auto-accept threshold, it is automatically
            accepted and its proposed text replaces the translation's published text, in the
            same operation as this vote.
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
