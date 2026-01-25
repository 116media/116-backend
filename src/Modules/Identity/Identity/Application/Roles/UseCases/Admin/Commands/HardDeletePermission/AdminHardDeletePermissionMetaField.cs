using _116.Shared.Application.Metadata;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.HardDeletePermission;

/// <summary>
/// Contains metadata information for the admin hard delete permission route.
/// </summary>
public static class AdminHardDeletePermissionMetaField
{
    public static readonly RouteMetadata AdminHardDeletePermission = new(
        "AdminHardDeletePermission",
        "Permanently delete a permission",
        """
            Permanently deletes a permission from the system. This action cannot be undone.
            \n
            This endpoint hard deletes a permission by:\n
            - Validating the permission ID exists\n
            - Permanently removing the permission from the database\n
            - Cascading deletion to role-permission associations\n
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Use Cases:**\n
            - Permanently remove obsolete permissions\n
            - Clean up test or temporary permissions\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on successful deletion\n
            - Returns 400 Bad Request if validation fails\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 404 Not Found if permission doesn't exist\n
            \n
            **Error Handling:**\n
            - ValidationException (400): Invalid permission ID format\n
            - NotFoundException (404): Permission not found with the specified ID
        """
    );
}
