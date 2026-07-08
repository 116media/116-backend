using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateShortVideo;

/// <summary>
/// Contains metadata information for the update short video route.
/// </summary>
public static class AdminUpdateShortVideoMetaField
{
    public static readonly RouteMetadata UpdateShortVideo = new(
        "UpdateShortVideo",
        "Update short video metadata and optionally replace the video file",
        """
            Updates the editable metadata of a short video (title, parent video link)
            and optionally replaces the video file. The slug is immutable after creation
            to preserve public URLs shared on social media.
            \n
            When a new video file is provided, it overwrites the existing file in cloud storage
            using the same storage key.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with updated short video details on success\n
            - Returns 400 Bad Request if validation fails\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
            - Returns 404 Not Found if the short video does not exist\n
            - Returns 429 Too Many Requests if the rate limit is exceeded\n
        """
    );
}
