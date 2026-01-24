using _116.Shared.Application.Metadata;

namespace _116.Identity.Application.Roles.UseCases.Admin.Queries.GetAllPermissions;

/// <summary>
/// Contains metadata information for the admin get all permissions route.
/// </summary>
public static class AdminGetAllPermissionsMetaField
{
    public static readonly RouteMetadata AdminGetAllPermissions = new(
        "AdminGetAllPermissions",
        "Retrieve all permissions with pagination and filtering",
        """
            Retrieves a paginated list of permissions with optional search and filtering.
            \n
            This endpoint provides permission listing by:\n
            - Supporting fuzzy search across Resource, Action, and Description (case-insensitive)\n
            - Filtering by active status (IsActive)\n
            - Filtering by deleted status (IsDeleted)\n
            - Paginating results for efficient data retrieval\n
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Query Parameters:**\n
            - pageIndex: Zero-based page index (default: 0)\n
            - pageSize: Number of items per page (default: 10)\n
            - search: Fuzzy search term for Resource, Action, and Description\n
            - isActive: Filter by active status (true/false)\n
            - isDeleted: Filter by deleted status (true/false)\n
            \n
            **Use Cases:**\n
            - List all available permissions in the system\n
            - Search for specific permissions by resource, action, or description\n
            - Filter permissions by status for administrative purposes\n
            - Paginate through large permission sets\n
            \n
            **Response Includes:**\n
            - Paginated list of permissions with ID, resource, action, description, and status\n
            - Total count for pagination\n
            - Current page index and page size\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with paginated permission list on success\n
            - Returns 400 Bad Request if query parameters are invalid\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks required permissions
        """
    );
}
