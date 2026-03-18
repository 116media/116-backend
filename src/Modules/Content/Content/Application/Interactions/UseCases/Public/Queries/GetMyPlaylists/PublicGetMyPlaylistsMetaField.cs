using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Interactions.UseCases.Public.Queries.GetMyPlaylists;

/// <summary>
/// Contains metadata information for the get my playlists route.
/// </summary>
public static class PublicGetMyPlaylistsMetaField
{
    public static readonly RouteMetadata PublicGetMyPlaylists = new(
        "PublicGetMyPlaylists",
        "Get my playlists",
        """
            Returns all playlists owned by the authenticated user.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have an active account\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
