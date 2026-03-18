using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.DeactivateShortVideo;

/// <summary>
/// Contains metadata information for the deactivate short video route.
/// </summary>
public static class AdminDeactivateShortVideoMetaField
{
    public static readonly RouteMetadata AdminDeactivateShortVideo = new(
        "DeactivateShortVideo",
        "Deactivate a short video",
        """
            Hides the specified short video from the public feed by clearing its active status.
            Deactivation is reversible and does not delete any media assets.
            \n
            Returns a conflict error if the short video is already inactive.
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
            - Returns 409 Conflict if the short video is already inactive\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
