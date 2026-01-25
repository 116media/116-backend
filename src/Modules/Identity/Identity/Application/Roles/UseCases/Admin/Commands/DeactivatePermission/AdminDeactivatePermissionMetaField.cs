using _116.Shared.Application.Metadata;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.DeactivatePermission;

/// <summary>
/// Contains metadata information for the admin deactivate permission route.
/// </summary>
public static class AdminDeactivatePermissionMetaField
{
    public static readonly RouteMetadata AdminDeactivatePermission = new(
        "AdminDeactivatePermission",
        "Deactivate a permission",
        """
            Deactivates a permission, preventing it from being assigned to roles.
            \n
            This endpoint deactivates a permission by:\n
            - Validating the permission ID exists\n
            - Checking that the permission is not already inactive\n
            - Setting the permission's IsActive status to false\n
            - Returning the updated permission details\n
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Use Cases:**\n
            - Temporarily disable a permission without deleting it\n
            - Prevent new role assignments of the permission\n
            - Existing role assignments remain unaffected\n
            \n
            **Response Includes:**\n
            - Permission ID, resource, action, and description\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with deactivated permission details on success\n
            - Returns 400 Bad Request if validation fails\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 404 Not Found if permission doesn't exist\n
            - Returns 409 Conflict if permission is already inactive\n
            \n
            **Error Handling:**\n
            - ValidationException (400): Invalid permission ID format\n
            - NotFoundException (404): Permission not found with the specified ID\n
            - ConflictException (409): Permission is already inactive
        """
    );
}
