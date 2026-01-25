using _116.Shared.Application.Metadata;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.BulkUpdateRolePermissions;

/// <summary>
/// Contains metadata information for the admin bulk update role permissions route.
/// </summary>
public static class AdminBulkUpdateRolePermissionsMetaField
{
    public static readonly RouteMetadata AdminBulkUpdateRolePermissions = new(
        "AdminBulkUpdateRolePermissions",
        "Bulk update role permissions",
        """
            Replaces all permissions of a role with the specified list.
            \n
            This endpoint bulk updates permissions by:\n
            - Validating the role ID exists\n
            - Removing all permissions not in the new list\n
            - Adding all permissions in the new list that aren't already assigned\n
            - Returning the updated role with all permissions\n
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Use Cases:**\n
            - Completely reset role permissions\n
            - Sync role permissions with an external system\n
            - Batch permission updates\n
            \n
            **Request Body:**\n
            - permissionIds: Array of permission IDs to assign to the role\n
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
            - Returns 404 Not Found if role doesn't exist\n
            \n
            **Error Handling:**\n
            - ValidationException (400): Invalid role ID or permission ID format\n
            - NotFoundException (404): Role not found
        """
    );
}
