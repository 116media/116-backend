using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadShortVideoThumbnail;

/// <summary>
/// Contains metadata information for the upload short video thumbnail route.
/// </summary>
public static class UploadShortVideoThumbnailMetaField
{
    public static readonly RouteMetadata UploadShortVideoThumbnail = new(
        "UploadShortVideoThumbnail",
        "Upload or replace the thumbnail for a short video",
        """
            Uploads an image file to Cloudinary and sets it as the thumbnail for the specified
            short video. If a thumbnail already exists, the previous image is deleted from
            cloud storage after the new one is successfully saved.
            \n
            The image file must be submitted as <c>multipart/form-data</c>.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with thumbnail URL and storage key on success\n
            - Returns 400 Bad Request if validation fails\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
            - Returns 404 Not Found if the short video does not exist\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
