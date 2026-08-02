using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Public.Commands.ProposeTranslationRevision;

/// <summary>
/// Contains metadata information for the propose translation revision route.
/// </summary>
public static class PublicProposeTranslationRevisionMetaField
{
    public static readonly RouteMetadata ProposeTranslationRevision = new(
        "ProposeTranslationRevision",
        "Propose a correction to a translation",
        """
            Proposes a community correction to a published translation's text, entering the
            Wikipedia-style community review workflow.
            \n
            The proposed text never replaces the published translation directly — only an
            accepted revision's later application does, once enough community votes are cast
            or a moderator overrides the tally.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have an active account\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on success\n
            - Returns 400 Bad Request if the proposed text is missing\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 404 Not Found if the translation does not exist\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
