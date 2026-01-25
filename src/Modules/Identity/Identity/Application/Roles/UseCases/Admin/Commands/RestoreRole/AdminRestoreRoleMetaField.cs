using _116.Shared.Application.Metadata;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.RestoreRole;

/// <summary>
/// Contains metadata information for the admin restore role route.
/// </summary>
public static class AdminRestoreRoleMetaField
{
    public static readonly RouteMetadata AdminRestoreRole = new(
        "AdminRestoreRole",
        "Restore a soft-deleted role",
        """
            Restores a soft-deleted role, making it available again.
            \n
            This endpoint restores a role by:\n
            - Validating the role ID exists\n
            - Checking that the role is currently soft-deleted\n
            - Setting the role's IsDeleted status to false\n
            - Clearing the deletion timestamp\n
            - Returning the restored role details\n
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Use Cases:**\n
            - Recover accidentally deleted roles\n
            - Reinstate previously removed roles\n
            \n
            **Note:**\n
            - The restored role will remain inactive (IsActive = false)\n
            - Use the activate endpoint to make the role assignable again\n
            \n
            **Response Includes:**\n
            - Role ID, name, and description\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with restored role details on success\n
            - Returns 400 Bad Request if validation fails\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 404 Not Found if role doesn't exist\n
            - Returns 409 Conflict if role is not deleted\n
            \n
            **Error Handling:**\n
            - ValidationException (400): Invalid role ID format\n
            - NotFoundException (404): Role not found with the specified ID\n
            - ConflictException (409): Role is not deleted and cannot be restored
        """
    );
}
