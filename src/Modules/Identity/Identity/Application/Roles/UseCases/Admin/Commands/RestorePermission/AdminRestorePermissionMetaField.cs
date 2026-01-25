using _116.Shared.Application.Metadata;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.RestorePermission;

/// <summary>
/// Contains metadata information for the admin restore permission route.
/// </summary>
public static class AdminRestorePermissionMetaField
{
    public static readonly RouteMetadata AdminRestorePermission = new(
        "AdminRestorePermission",
        "Restore a soft-deleted permission",
        """
            Restores a soft-deleted permission, making it available again.
            \n
            This endpoint restores a permission by:\n
            - Validating the permission ID exists\n
            - Checking that the permission is currently soft-deleted\n
            - Setting the permission's IsDeleted status to false\n
            - Clearing the deletion timestamp\n
            - Returning the restored permission details\n
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Use Cases:**\n
            - Recover accidentally deleted permissions\n
            - Reinstate previously removed permissions\n
            \n
            **Note:**\n
            - The restored permission will remain inactive (IsActive = false)\n
            - Use the activate endpoint to make the permission usable again\n
            \n
            **Response Includes:**\n
            - Permission ID, resource, action, and description\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with restored permission details on success\n
            - Returns 400 Bad Request if validation fails\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 404 Not Found if permission doesn't exist\n
            - Returns 409 Conflict if permission is not deleted\n
            \n
            **Error Handling:**\n
            - ValidationException (400): Invalid permission ID format\n
            - NotFoundException (404): Permission not found with the specified ID\n
            - ConflictException (409): Permission is not deleted and cannot be restored
        """
    );
}
