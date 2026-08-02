using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateAlbum;

/// <summary>
/// Contains metadata information for the create album route.
/// </summary>
public static class AdminCreateAlbumMetaField
{
    public static readonly RouteMetadata CreateAlbum = new(
        "CreateAlbum",
        "Create a new album",
        """
            Creates a new album, optionally linked to a claimed artist profile.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 201 Created with the album on success\n
            - Returns 400 Bad Request if the payload fails validation\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 404 Not Found if the linked artist profile does not exist\n
            - Returns 429 Too Many Requests if the rate limit is exceeded\n
        """
    );
}
