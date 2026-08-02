using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateAlbum;

/// <summary>
/// Contains metadata information for the update album route.
/// </summary>
public static class AdminUpdateAlbumMetaField
{
    public static readonly RouteMetadata UpdateAlbum = new(
        "UpdateAlbum",
        "Update an album",
        """
            Updates an album's display name, release year, and record label. The cover image
            is managed separately via the dedicated cover upload endpoint.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with the updated album on success\n
            - Returns 400 Bad Request if the payload fails validation\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 404 Not Found if the album does not exist\n
            - Returns 429 Too Many Requests if the rate limit is exceeded\n
        """
    );
}
