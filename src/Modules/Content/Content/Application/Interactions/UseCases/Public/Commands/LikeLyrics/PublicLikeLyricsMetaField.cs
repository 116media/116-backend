using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.LikeLyrics;

/// <summary>
/// Contains metadata information for the like lyrics route.
/// </summary>
public static class PublicLikeLyricsMetaField
{
    public static readonly RouteMetadata LikeLyrics = new(
        "PublicLikeLyrics",
        "Like a lyrics page",
        """
            Records that the authenticated user has liked a lyrics page.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have an active account\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 404 Not Found if the lyrics page does not exist\n
            - Returns 409 Conflict if the user has already liked this lyrics page\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
