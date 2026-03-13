using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.AttachYoutubeId;

/// <summary>
/// Contains metadata information for the attach YouTube ID route.
/// </summary>
public static class AttachYoutubeIdMetaField
{
    public static readonly RouteMetadata AttachYoutubeId = new(
        "AttachYoutubeId",
        "Attach a YouTube video ID",
        """
            Attaches a YouTube video ID to a video and automatically downloads
            the YouTube thumbnail, re-uploading it to Cloudinary.
            \n
            If the video already has a thumbnail, the old Cloudinary asset is deleted
            after the new thumbnail is uploaded successfully.
            \n
            The YouTube thumbnail is first attempted at maxresdefault quality (1280x720),
            falling back to hqdefault (480x360) if unavailable.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with updated video details on success\n
            - Returns 400 Bad Request if validation fails\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
            - Returns 404 Not Found if the video does not exist\n
        """
    );
}
