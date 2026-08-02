using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.LinkLyricsArtist;

/// <summary>
/// Contains metadata information for the link lyrics artist route.
/// </summary>
public static class AdminLinkLyricsArtistMetaField
{
    public static readonly RouteMetadata LinkLyricsArtist = new(
        "LinkLyricsArtist",
        "Link or unlink a lyrics page's artist profile",
        """
            Links a lyrics page to a real, addressable artist profile, or unlinks it when no
            artist id is supplied. The lyrics page's plain-text artist name is preserved
            either way.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
            - Returns 404 Not Found if the lyrics page or artist profile does not exist\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
