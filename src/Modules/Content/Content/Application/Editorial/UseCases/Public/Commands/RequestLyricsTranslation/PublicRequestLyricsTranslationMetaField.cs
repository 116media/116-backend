using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Public.Commands.RequestLyricsTranslation;

/// <summary>
/// Contains metadata information for the request lyrics translation route.
/// </summary>
public static class PublicRequestLyricsTranslationMetaField
{
    public static readonly RouteMetadata RequestLyricsTranslation = new(
        "RequestLyricsTranslation",
        "Request an AI-generated translation of a lyrics page",
        """
            Requests a translation of a lyrics page into the given language.
            \n
            Idempotent: if a translation already exists for the lyrics page and language, it is
            returned as-is without generating a new one. Otherwise an AI translation is generated
            and stored as the lyrics page's first translation into that language.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have an active account\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 404 Not Found if the lyrics page does not exist\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
