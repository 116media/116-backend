using _116.Shared.Application.Metadata;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.ActivatePermission;

/// <summary>
/// Contains metadata information for the admin activate permission route.
/// </summary>
public static class AdminActivatePermissionMetaField
{
    public static readonly RouteMetadata AdminActivatePermission = new(
        "AdminActivatePermission",
        "Activate a permission",
        """
            Activates a permission, allowing it to be assigned to roles.
            \n
            This endpoint activates a permission by:\n
            - Validating the permission ID exists\n
            - Checking that the permission is not already active\n
            - Setting the permission's IsActive status to true\n
            - Returning the updated permission details\n
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Use Cases:**\n
            - Re-enable a previously deactivated permission\n
            - Make a permission available for assignment to roles\n
            \n
            **Response Includes:**\n
            - Permission ID, resource, action, and description\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with activated permission details on success\n
            - Returns 400 Bad Request if validation fails\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 404 Not Found if permission doesn't exist\n
            - Returns 409 Conflict if permission is already active\n
            \n
            **Error Handling:**\n
            - ValidationException (400): Invalid permission ID format\n
            - NotFoundException (404): Permission not found with the specified ID\n
            - ConflictException (409): Permission is already active
        """
    );
}
