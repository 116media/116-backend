using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Public.Commands.ProposeLyricsRevision;

/// <summary>
/// Contains metadata information for the propose lyrics revision route.
/// </summary>
public static class PublicProposeLyricsRevisionMetaField
{
    public static readonly RouteMetadata ProposeLyricsRevision = new(
        "ProposeLyricsRevision",
        "Propose a correction to a lyrics page",
        """
            Proposes a community correction to a lyrics page's canonical text, entering the
            Wikipedia-style community review workflow.
            \n
            Applies uniformly to every lyrics page regardless of how it was created —
            admin-entered, community-submitted, or verified-artist self-uploaded — there is no
            trust exemption based on origin.
            \n
            The proposed text never replaces the published text directly — only an accepted
            revision's later application does, once enough community votes are cast or a
            moderator overrides the tally.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have an active account\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on success\n
            - Returns 400 Bad Request if the proposed text is missing\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 404 Not Found if the lyrics page does not exist\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
