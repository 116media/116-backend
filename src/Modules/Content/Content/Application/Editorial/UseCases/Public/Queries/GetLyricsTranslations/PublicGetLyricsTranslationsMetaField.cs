using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsTranslations;

/// <summary>
/// Contains metadata information for the get lyrics translations route.
/// </summary>
public static class PublicGetLyricsTranslationsMetaField
{
    public static readonly RouteMetadata GetLyricsTranslations = new(
        "GetLyricsTranslations",
        "List a lyrics page's translations",
        """
            Retrieves every translation of a lyrics page, one per requested language.
            \n
            **Authentication Requirements:**\n
            - None — publicly accessible\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on success\n
            - Returns 404 Not Found if the lyrics page does not exist\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
