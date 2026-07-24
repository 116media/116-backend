using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.UnlikeLyrics;

/// <summary>
/// Contains metadata information for the unlike lyrics route.
/// </summary>
public static class PublicUnlikeLyricsMetaField
{
    public static readonly RouteMetadata UnlikeLyrics = new(
        "PublicUnlikeLyrics",
        "Unlike a lyrics page",
        """
            Removes the authenticated user's like from a lyrics page.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have an active account\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on success\n
            - Returns 400 Bad Request if the user has not liked this lyrics page\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 404 Not Found if the lyrics page does not exist\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
