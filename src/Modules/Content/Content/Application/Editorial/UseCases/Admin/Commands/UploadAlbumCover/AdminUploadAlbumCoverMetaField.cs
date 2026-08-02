using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadAlbumCover;

/// <summary>
/// Contains metadata information for the upload album cover route.
/// </summary>
public static class AdminUploadAlbumCoverMetaField
{
    public static readonly RouteMetadata UploadAlbumCover = new(
        "UploadAlbumCover",
        "Upload an album cover art image",
        """
            Uploads or replaces the album's cover art image.
            \n
            If the album already has a cover image, the old Cloudinary asset is deleted
            after the new cover is uploaded successfully.
            \n
            Accepts multipart/form-data with a single image file.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with cover image URL and storage key on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
            - Returns 404 Not Found if the album does not exist\n
            - Returns 429 Too Many Requests if the rate limit is exceeded\n
        """
    );
}
