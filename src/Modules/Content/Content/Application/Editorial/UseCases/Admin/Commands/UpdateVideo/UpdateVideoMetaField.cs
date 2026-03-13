using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideo;

/// <summary>
/// Contains metadata information for the update video route.
/// </summary>
public static class UpdateVideoMetaField
{
    public static readonly RouteMetadata UpdateVideo = new(
        "UpdateVideo",
        "Update video metadata",
        """
            Updates a video's editable metadata fields (title, slug, description).
            \n
            Only videos in <c>Draft</c> or <c>Rejected</c> status can be updated.
            Attempting to update a video in any other status will return a 400 Bad Request.
            \n
            If the slug is changed, the new slug must be unique across all videos.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with updated video details on success\n
            - Returns 400 Bad Request if validation fails or video is not in an editable status\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
            - Returns 404 Not Found if the video does not exist\n
            - Returns 409 Conflict if the new slug is already taken\n
        """
    );
}
