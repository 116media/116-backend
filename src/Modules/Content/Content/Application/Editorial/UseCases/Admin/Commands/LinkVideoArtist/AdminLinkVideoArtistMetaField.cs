using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.LinkVideoArtist;

/// <summary>
/// Contains metadata information for the link video artist route.
/// </summary>
public static class AdminLinkVideoArtistMetaField
{
    public static readonly RouteMetadata LinkVideoArtist = new(
        "LinkVideoArtist",
        "Link or unlink a video's artist profile",
        """
            Links a video to a real, addressable artist profile, or unlinks it when no
            artist id is supplied.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
            - Returns 404 Not Found if the video or artist profile does not exist\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
