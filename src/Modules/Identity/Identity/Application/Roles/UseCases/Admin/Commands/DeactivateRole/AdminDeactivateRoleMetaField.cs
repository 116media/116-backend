using _116.Shared.Application.Metadata;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.DeactivateRole;

/// <summary>
/// Contains metadata information for the admin deactivate role route.
/// </summary>
public static class AdminDeactivateRoleMetaField
{
    public static readonly RouteMetadata AdminDeactivateRole = new(
        "AdminDeactivateRole",
        "Deactivate a role",
        """
            Deactivates a role, preventing it from being assigned to users.
            \n
            This endpoint deactivates a role by:\n
            - Validating the role ID exists\n
            - Checking that the role is not already inactive\n
            - Setting the role's IsActive status to false\n
            - Returning the updated role details\n
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Use Cases:**\n
            - Temporarily disable a role without deleting it\n
            - Prevent new users from being assigned the role\n
            - Existing user assignments remain unaffected\n
            \n
            **Response Includes:**\n
            - Role ID, name, and description\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with deactivated role details on success\n
            - Returns 400 Bad Request if validation fails\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 404 Not Found if role doesn't exist\n
            - Returns 409 Conflict if role is already inactive\n
            \n
            **Error Handling:**\n
            - ValidationException (400): Invalid role ID format\n
            - NotFoundException (404): Role not found with the specified ID\n
            - ConflictException (409): Role is already inactive
        """
    );
}
