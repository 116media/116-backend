using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateShortVideo;

/// <summary>
/// Contains metadata information for the update short video route.
/// </summary>
public static class AdminUpdateShortVideoMetaField
{
    public static readonly RouteMetadata AdminUpdateShortVideo = new(
        "UpdateShortVideo",
        "Update short video metadata",
        """
            Updates the editable metadata fields of a short video (title, slug, parent video link).
            \n
            If the slug is changed, the new slug must be unique across all short videos.
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
            - Returns 409 Conflict if the new slug is already taken\n
            - Returns 429 Too Many Requests if the rate limit is exceeded\n
        """
    );
}
