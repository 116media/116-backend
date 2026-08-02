using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetTranslationRevisions;

/// <summary>
/// Contains metadata information for the get translation revisions route.
/// </summary>
public static class PublicGetTranslationRevisionsMetaField
{
    public static readonly RouteMetadata GetTranslationRevisions = new(
        "GetTranslationRevisions",
        "List a translation's full revision history",
        """
            Retrieves every revision ever proposed against a translation — pending, accepted,
            and rejected alike — newest first.
            \n
            **Authentication Requirements:**\n
            - None — publicly accessible\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on success\n
            - Returns 404 Not Found if the translation does not exist\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
