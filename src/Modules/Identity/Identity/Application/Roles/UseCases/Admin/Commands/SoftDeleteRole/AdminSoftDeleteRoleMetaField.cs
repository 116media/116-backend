using _116.Shared.Application.Metadata;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.SoftDeleteRole;

/// <summary>
/// Contains metadata information for the admin soft delete role route.
/// </summary>
public static class AdminSoftDeleteRoleMetaField
{
    public static readonly RouteMetadata AdminSoftDeleteRole = new(
        "AdminSoftDeleteRole",
        "Soft delete a role",
        """
            Soft deletes a role, marking it as deleted without permanent removal.
            \n
            This endpoint soft deletes a role by:\n
            - Validating the role ID exists\n
            - Checking that the role is not already deleted\n
            - Setting the role's IsDeleted status to true and IsActive to false\n
            - Recording the deletion timestamp\n
            - Returning the updated role details\n
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Use Cases:**\n
            - Remove a role while preserving the ability to restore it\n
            - Hide role from active listings\n
            - Maintain audit trail of deleted roles\n
            \n
            **Response Includes:**\n
            - Role ID, name, and description\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with soft deleted role details on success\n
            - Returns 400 Bad Request if validation fails\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 404 Not Found if role doesn't exist\n
            - Returns 409 Conflict if role is already deleted\n
            \n
            **Error Handling:**\n
            - ValidationException (400): Invalid role ID format\n
            - NotFoundException (404): Role not found with the specified ID\n
            - ConflictException (409): Role is already deleted
        """
    );
}
