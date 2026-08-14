using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ResolveSingleStreamingLinks;

/// <summary>
/// Contains metadata information for the resolve album streaming links route.
/// </summary>
public static class AdminResolveSingleStreamingLinksMetaField
{
    public static readonly RouteMetadata ResolveSingleStreamingLinks = new(
        "ResolveSingleStreamingLinks",
        "Resolve a standalone single's streaming links from one platform URL",
        """
            Sends one verified platform URL to the external link-resolution provider
            (Odesli) and upserts a curated deep link for every platform it matches, in a
            single atomic commit. Platforms the provider has no link for are left untouched
            — their generated search-URL fallback keeps serving, and resolution never
            deletes an existing curated row.
            \n
            The source URL must be an absolute https track link on a supported platform. A
            song that belongs to an album is rejected — the album's links are the release's
            links.
            \n
            **Authentication Requirements:**\n
            - Requires authentication with an active account\n
            - Requires Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with the resolved and unresolved platform lists on success\n
            - Returns 400 Bad Request if the source URL is missing, non-https, or unrecognised\n
            - Returns 409 Conflict if the song belongs to an album\n
            - Returns 401 Unauthorized if not authenticated\n
            - Returns 403 Forbidden if lacking the required role\n
            - Returns 404 Not Found if the song does not exist, or the provider found no platforms\n
            - Returns 429 Too Many Requests if our rate limit or the provider's is exceeded\n
            - Returns 502 Bad Gateway if the provider is unreachable\n
        """
    );
}
