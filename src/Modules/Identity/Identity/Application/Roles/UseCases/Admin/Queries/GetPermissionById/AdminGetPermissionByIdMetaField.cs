using _116.Shared.Application.Metadata;

namespace _116.Identity.Application.Roles.UseCases.Admin.Queries.GetPermissionById;

/// <summary>
/// Contains metadata information for the admin get permission by ID route.
/// </summary>
public static class AdminGetPermissionByIdMetaField
{
    public static readonly RouteMetadata AdminGetPermissionById = new(
        "AdminGetPermissionById",
        "Retrieve a permission by ID",
        """
            Retrieves detailed information about a specific permission identified by its ID.
            \n
            This endpoint provides permission details by:\n
            - Validating the permission ID from the route parameter\n
            - Fetching the permission data\n
            - Returning complete permission metadata\n
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Includes:**\n
            - Permission ID, resource, action, and description\n
            - Permission status (IsActive, IsDeleted, DeletedAt)\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with permission details on success\n
            - Returns 400 Bad Request if permission ID is invalid\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks required permissions\n
            - Returns 404 Not Found if permission doesn't exist\n
            \n
            **Error Handling:**\n
            - NotFoundException (404): Permission not found with the specified ID
        """
    );
}
