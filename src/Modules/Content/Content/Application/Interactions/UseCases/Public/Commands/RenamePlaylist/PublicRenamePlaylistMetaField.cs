using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.RenamePlaylist;

/// <summary>
/// Contains metadata information for the rename playlist route.
/// </summary>
public static class PublicRenamePlaylistMetaField
{
    public static readonly RouteMetadata PublicRenamePlaylist = new(
        "PublicRenamePlaylist",
        "Rename a playlist",
        """
            Updates the display name of a playlist owned by the authenticated user.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have an active account\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on success\n
            - Returns 400 Bad Request if validation fails or user is not the owner\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 404 Not Found if the playlist does not exist\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
