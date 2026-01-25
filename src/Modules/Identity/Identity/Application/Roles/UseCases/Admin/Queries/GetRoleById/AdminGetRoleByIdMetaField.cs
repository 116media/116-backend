using _116.Shared.Application.Metadata;

namespace _116.Identity.Application.Roles.UseCases.Admin.Queries.GetRoleById;

/// <summary>
/// Contains metadata information for the admin get role by ID route.
/// </summary>
public static class AdminGetRoleByIdMetaField
{
    public static readonly RouteMetadata AdminGetRoleById = new(
        "AdminGetRoleById",
        "Retrieve a role by ID with its permissions",
        """
            Retrieves detailed information about a specific role identified by its ID,
            including all permissions assigned to the role.
            \n
            This endpoint provides role details by:\n
            - Validating the role ID from the route parameter\n
            - Fetching the role with all associated permissions\n
            - Returning complete role metadata and permission list\n
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Use Cases:**\n
            - View detailed information about a specific role\n
            - Review permissions assigned to a role\n
            - Audit role configuration\n
            - Display role details in admin dashboard\n
            \n
            **Response Includes:**\n
            - Role ID, name, and description\n
            - Role status (IsActive, IsDeleted, DeletedAt)\n
            - List of all permissions assigned to the role\n
            - Each permission includes resource, action, and description\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with role details and permissions on success\n
            - Returns 400 Bad Request if role ID is invalid\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks required permissions\n
            - Returns 404 Not Found if role doesn't exist\n
            \n
            **Error Handling:**\n
            - NotFoundException (404): Role not found with the specified ID
        """
    );
}
