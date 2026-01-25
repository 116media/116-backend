using _116.Shared.Application.Metadata;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.AssignPermissionToRole;

/// <summary>
/// Contains metadata information for the admin assign permission to role route.
/// </summary>
public static class AdminAssignPermissionToRoleMetaField
{
    public static readonly RouteMetadata AdminAssignPermissionToRole = new(
        "AdminAssignPermissionToRole",
        "Assign a permission to a role",
        """
            Assigns a permission to a role, granting that permission to all users with the role.
            \n
            This endpoint assigns a permission by:\n
            - Validating the role ID exists\n
            - Validating the permission ID exists\n
            - Checking that the permission is not already assigned to the role\n
            - Creating the role-permission association\n
            - Returning the updated role with all permissions\n
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Use Cases:**\n
            - Grant new capabilities to a role\n
            - Expand role access to new resources\n
            \n
            **Request Body:**\n
            - permissionId: The ID of the permission to assign\n
            \n
            **Response Includes:**\n
            - Role ID, name, and description\n
            - List of all assigned permissions\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with updated role details on success\n
            - Returns 400 Bad Request if validation fails\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 404 Not Found if role or permission doesn't exist\n
            - Returns 409 Conflict if permission is already assigned to role\n
            \n
            **Error Handling:**\n
            - ValidationException (400): Invalid role ID or permission ID format\n
            - NotFoundException (404): Role or permission not found\n
            - ConflictException (409): Permission is already assigned to the role
        """
    );
}
