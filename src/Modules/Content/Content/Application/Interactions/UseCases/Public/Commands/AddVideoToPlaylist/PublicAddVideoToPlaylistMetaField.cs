using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.AddVideoToPlaylist;

/// <summary>
/// Contains metadata information for the add video to playlist route.
/// </summary>
public static class PublicAddVideoToPlaylistMetaField
{
    public static readonly RouteMetadata PublicAddVideoToPlaylist = new(
        "PublicAddVideoToPlaylist",
        "Add a video to a playlist",
        """
            Adds a published video to the authenticated user's playlist.
            Returns 409 Conflict if the video is already in the playlist.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have an active account\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on success\n
            - Returns 400 Bad Request if the requesting user is not the playlist owner\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 404 Not Found if the playlist or video does not exist\n
            - Returns 409 Conflict if the video is already in the playlist\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
