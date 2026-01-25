using _116.Shared.Application.Metadata;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.SoftDeletePermission;

/// <summary>
/// Contains metadata information for the admin soft delete permission route.
/// </summary>
public static class AdminSoftDeletePermissionMetaField
{
    public static readonly RouteMetadata AdminSoftDeletePermission = new(
        "AdminSoftDeletePermission",
        "Soft delete a permission",
        """
            Soft deletes a permission, marking it as deleted without permanent removal.
            \n
            This endpoint soft deletes a permission by:\n
            - Validating the permission ID exists\n
            - Checking that the permission is not already deleted\n
            - Setting the permission's IsDeleted status to true and IsActive to false\n
            - Recording the deletion timestamp\n
            - Returning the updated permission details\n
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Use Cases:**\n
            - Remove a permission while preserving the ability to restore it\n
            - Hide permission from active listings\n
            - Maintain audit trail of deleted permissions\n
            \n
            **Response Includes:**\n
            - Permission ID, resource, action, and description\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with soft deleted permission details on success\n
            - Returns 400 Bad Request if validation fails\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 404 Not Found if permission doesn't exist\n
            - Returns 409 Conflict if permission is already deleted\n
            \n
            **Error Handling:**\n
            - ValidationException (400): Invalid permission ID format\n
            - NotFoundException (404): Permission not found with the specified ID\n
            - ConflictException (409): Permission is already deleted
        """
    );
}
