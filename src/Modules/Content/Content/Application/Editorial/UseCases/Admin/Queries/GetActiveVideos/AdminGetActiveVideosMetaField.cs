using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Queries.GetActiveVideos;

/// <summary>
/// Contains metadata information for the admin get active videos route.
/// </summary>
public static class AdminGetActiveVideosMetaField
{
    public static readonly RouteMetadata GetActiveVideos = new(
        "AdminGetActiveVideos",
        "List all active videos (excludes Archived and Rejected)",
        """
            Retrieves all active videos (excludes Archived and Rejected).
            Returns an unpaginated list for use in dropdowns and selection fields.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with the list of active videos on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
        """
    );
}
