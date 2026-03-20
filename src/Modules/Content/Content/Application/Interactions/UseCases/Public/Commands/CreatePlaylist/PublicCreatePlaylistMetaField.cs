using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.CreatePlaylist;

/// <summary>
/// Contains metadata information for the create playlist route.
/// </summary>
public static class PublicCreatePlaylistMetaField
{
    public static readonly RouteMetadata PublicCreatePlaylist = new(
        "PublicCreatePlaylist",
        "Create a new playlist",
        """
            Creates a new playlist owned by the authenticated user.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have an active account\n
            \n
            **Response Codes:**\n
            - Returns 201 Created on success with the playlist DTO\n
            - Returns 400 Bad Request if validation fails\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
