using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.DeleteVideo;

/// <summary>
/// Contains metadata information for the delete video route.
/// </summary>
public static class DeleteVideoMetaField
{
    public static readonly RouteMetadata DeleteVideo = new(
        "DeleteVideo",
        "Permanently delete a video",
        """
            Permanently deletes a video and its associated Cloudinary thumbnail asset.
            \n
            Only videos in <c>Draft</c> or <c>Rejected</c> status can be deleted.
            Attempting to delete a video in any other status (Published, Approved,
            PendingReview, PendingPayment) will return a 400 Bad Request.
            \n
            For published or approved videos, use the archive endpoint instead to
            remove the video from public feeds without permanently deleting it.
            \n
            This operation is <b>irreversible</b> — the Cloudinary thumbnail asset is deleted
            from cloud storage before the database record is removed.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 204 No Content on success\n
            - Returns 400 Bad Request if the video is not in Draft or Rejected status\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 404 Not Found if the video does not exist\n
        """
    );
}
