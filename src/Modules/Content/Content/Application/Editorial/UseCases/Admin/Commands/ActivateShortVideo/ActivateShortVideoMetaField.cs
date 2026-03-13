using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ActivateShortVideo;

/// <summary>
/// Contains metadata information for the activate short video route.
/// </summary>
public static class ActivateShortVideoMetaField
{
    public static readonly RouteMetadata ActivateShortVideo = new(
        "ActivateShortVideo",
        "Activate a short video",
        """
            Makes the specified short video visible on the public feed by setting its active status.
            \n
            Returns a conflict error if the short video is already active.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 204 No Content on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 404 Not Found if the short video does not exist\n
            - Returns 409 Conflict if the short video is already active\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
