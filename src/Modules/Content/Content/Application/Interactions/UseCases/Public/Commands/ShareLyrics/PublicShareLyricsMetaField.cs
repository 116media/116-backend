using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.ShareLyrics;

/// <summary>
/// Contains metadata information for the share lyrics route.
/// </summary>
public static class PublicShareLyricsMetaField
{
    public static readonly RouteMetadata ShareLyrics = new(
        "PublicShareLyrics",
        "Share a lyrics page",
        """
            Records a share event for a lyrics page. Works for both authenticated and
            anonymous callers.
            \n
            **Authentication Requirements:**\n
            - No authentication required (public endpoint)\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on success\n
            - Returns 404 Not Found if the lyrics page does not exist\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
