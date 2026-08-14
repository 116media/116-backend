using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.SetArticleArtists;

/// <summary>
/// Contains metadata information for the set article artists route.
/// </summary>
public static class AdminSetArticleArtistsMetaField
{
    public static readonly RouteMetadata SetArticleArtists = new(
        "SetArticleArtists",
        "Set-replace the artists an article is tagged with",
        """
            Replaces the article's artist tags with the given complete set: rows not in the
            list are removed, rows already present are kept, new ones are added. An empty
            list untags everything.
            \n
            Every artist id must reference an existing artist profile; the error names the
            first missing id.
            \n
            **Authentication Requirements:**\n
            - Requires authentication with an active account\n
            - Requires Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with the article's artist ids after the replace\n
            - Returns 400 Bad Request if the list is missing, oversized, or has duplicates\n
            - Returns 401 Unauthorized if not authenticated\n
            - Returns 403 Forbidden if lacking the required role\n
            - Returns 404 Not Found if the article or any artist id does not exist\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
