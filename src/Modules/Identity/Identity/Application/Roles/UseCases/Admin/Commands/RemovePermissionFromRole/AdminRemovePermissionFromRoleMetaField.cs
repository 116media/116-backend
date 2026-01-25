using _116.Shared.Application.Metadata;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.RemovePermissionFromRole;

/// <summary>
/// Contains metadata information for the admin remove permission from role route.
/// </summary>
public static class AdminRemovePermissionFromRoleMetaField
{
    public static readonly RouteMetadata AdminRemovePermissionFromRole = new(
        "AdminRemovePermissionFromRole",
        "Remove a permission from a role",
        """
            Removes a permission from a role, revoking that permission from all users with the role.
            \n
            This endpoint removes a permission by:\n
            - Validating the role ID exists\n
            - Checking that the permission is assigned to the role\n
            - Removing the role-permission association\n
            - Returning the updated role with remaining permissions\n
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Use Cases:**\n
            - Revoke capabilities from a role\n
            - Reduce role access to resources\n
            \n
            **Response Includes:**\n
            - Role ID, name, and description\n
            - List of remaining assigned permissions\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with updated role details on success\n
            - Returns 400 Bad Request if permission is not assigned to role\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 404 Not Found if role doesn't exist\n
            \n
            **Error Handling:**\n
            - ValidationException (400): Invalid role ID or permission ID format\n
            - BadRequestException (400): Permission is not assigned to the role\n
            - NotFoundException (404): Role not found
        """
    );
}
