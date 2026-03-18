using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.DeleteShortVideo;

/// <summary>
/// Contains metadata information for the delete short video route.
/// </summary>
public static class AdminDeleteShortVideoMetaField
{
    public static readonly RouteMetadata AdminDeleteShortVideo = new(
        "DeleteShortVideo",
        "Permanently delete a short video",
        """
            Permanently deletes the specified short video record and removes all associated
            media assets (video file and thumbnail, if present) from cloud storage.
            This operation is irreversible.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 404 Not Found if the short video does not exist\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
