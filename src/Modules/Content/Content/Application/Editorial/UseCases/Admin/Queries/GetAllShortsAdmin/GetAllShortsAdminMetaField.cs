using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Queries.GetAllShortsAdmin;

/// <summary>
/// Contains metadata information for the get all shorts admin route.
/// </summary>
public static class GetAllShortsAdminMetaField
{
    public static readonly RouteMetadata GetAllShortsAdmin = new(
        "GetAllShortsAdmin",
        "List all short videos",
        """
            Retrieves a paginated list of all short video clips for admin management.
            \n
            Supports optional filtering by active status. Results are returned as a paginated list
            suitable for list and management views.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with paginated short video list on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
