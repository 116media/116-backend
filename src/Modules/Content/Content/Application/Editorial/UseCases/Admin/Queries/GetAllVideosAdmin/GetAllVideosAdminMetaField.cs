using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Queries.GetAllVideosAdmin;

/// <summary>
/// Contains metadata information for the get all videos admin route.
/// </summary>
public static class GetAllVideosAdminMetaField
{
    public static readonly RouteMetadata GetAllVideosAdmin = new(
        "GetAllVideosAdmin",
        "List all videos",
        """
            Retrieves a paginated list of videos for admin management.
            Supports optional filtering by content status and category.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with paginated video list on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
        """
    );
}
