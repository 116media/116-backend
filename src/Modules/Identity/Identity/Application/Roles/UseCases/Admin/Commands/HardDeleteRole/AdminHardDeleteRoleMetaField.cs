using _116.Shared.Application.Metadata;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.HardDeleteRole;

/// <summary>
/// Contains metadata information for the admin hard delete role route.
/// </summary>
public static class AdminHardDeleteRoleMetaField
{
    public static readonly RouteMetadata AdminHardDeleteRole = new(
        "AdminHardDeleteRole",
        "Permanently delete a role",
        """
            Permanently deletes a role from the system. This action cannot be undone.
            \n
            This endpoint hard deletes a role by:\n
            - Validating the role ID exists\n
            - Checking that the role is not a core system role (SuperAdmin, Admin, Visitor)\n
            - Permanently removing the role from the database\n
            - Cascading deletion to role-permission associations\n
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Use Cases:**\n
            - Permanently remove obsolete roles\n
            - Clean up test or temporary roles\n
            \n
            **Protected Roles:**\n
            - SuperAdmin, Admin, and Visitor roles cannot be hard deleted\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on successful deletion\n
            - Returns 400 Bad Request if attempting to delete a core role\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 404 Not Found if role doesn't exist\n
            \n
            **Error Handling:**\n
            - ValidationException (400): Invalid role ID format or core role protection\n
            - NotFoundException (404): Role not found with the specified ID
        """
    );
}
