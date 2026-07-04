using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadShortVideoFile;

/// <summary>
/// Contains metadata information for the upload short video file route.
/// </summary>
public static class AdminUploadShortVideoFileMetaField
{
    public static readonly RouteMetadata UploadShortVideoFile = new(
        "UploadShortVideoFile",
        "Upload or replace the video file for a short video",
        """
            Uploads a video file to Cloudinary and sets it as the source file for the specified
            short video. If a video file already exists, the previous file is deleted from
            cloud storage after the new one is successfully saved.
            \n
            A short video is created as a draft without a file; this endpoint attaches the file
            so the short video becomes eligible for activation and visible in the feed.
            \n
            The video file must be submitted as <c>multipart/form-data</c>.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with video URL and storage key on success\n
            - Returns 400 Bad Request if validation fails\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
            - Returns 404 Not Found if the short video does not exist\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
