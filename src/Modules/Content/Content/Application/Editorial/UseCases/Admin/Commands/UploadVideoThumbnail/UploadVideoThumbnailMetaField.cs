using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadVideoThumbnail;

/// <summary>
/// Contains metadata information for the upload video thumbnail route.
/// </summary>
public static class UploadVideoThumbnailMetaField
{
    public static readonly RouteMetadata UploadVideoThumbnail = new(
        "UploadVideoThumbnail",
        "Upload a custom video thumbnail",
        """
            Uploads or replaces the video's thumbnail image.
            \n
            If the video already has a thumbnail (from a previous upload or from the
            YouTube thumbnail auto-download), the old Cloudinary asset is deleted after
            the new thumbnail is uploaded successfully.
            \n
            Accepts multipart/form-data with a single image file.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with thumbnail URL and storage key on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
            - Returns 404 Not Found if the video does not exist\n
            - Returns 429 Too Many Requests if the rate limit is exceeded\n
        """
    );
}
