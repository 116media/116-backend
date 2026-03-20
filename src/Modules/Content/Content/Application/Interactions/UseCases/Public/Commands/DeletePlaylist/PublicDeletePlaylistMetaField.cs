using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.DeletePlaylist;

/// <summary>
/// Contains metadata information for the delete playlist route.
/// </summary>
public static class PublicDeletePlaylistMetaField
{
    public static readonly RouteMetadata PublicDeletePlaylist = new(
        "PublicDeletePlaylist",
        "Delete a playlist",
        """
            Permanently deletes a playlist owned by the authenticated user.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have an active account\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on success\n
            - Returns 400 Bad Request if the requesting user is not the playlist owner\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 404 Not Found if the playlist does not exist\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
