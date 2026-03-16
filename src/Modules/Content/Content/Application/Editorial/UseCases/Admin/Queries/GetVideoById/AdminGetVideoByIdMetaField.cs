using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Queries.GetVideoById;

/// <summary>
/// Contains metadata information for the admin get video by id route.
/// </summary>
public static class AdminGetVideoByIdMetaField
{
    public static readonly RouteMetadata AdminGetVideoById = new(
        "AdminGetVideoById",
        "Get video details by ID",
        """
            Retrieves the full details of a single video including its tags,
            SEO metadata, thumbnail, YouTube ID, and workflow status.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with video details on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
            - Returns 404 Not Found if the video does not exist\n
        """
    );
}
