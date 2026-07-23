using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.SetLyricsTags;

/// <summary>
/// Contains metadata information for the set lyrics tags route.
/// </summary>
public static class AdminSetLyricsTagsMetaField
{
    public static readonly RouteMetadata SetLyricsTags = new(
        "SetLyricsTags",
        "Replace the tags applied to a lyrics page",
        """
            Replaces the complete set of tags assigned to a lyrics page.
            \n
            All existing tag associations are removed and replaced with the provided list of
            tag IDs. Passing an empty list removes all tags from the lyrics page.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
            - Returns 404 Not Found if the lyrics page does not exist\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
