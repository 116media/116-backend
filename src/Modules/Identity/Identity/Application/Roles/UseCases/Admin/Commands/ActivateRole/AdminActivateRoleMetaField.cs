using _116.Shared.Application.Metadata;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.ActivateRole;

/// <summary>
/// Contains metadata information for the admin activate role route.
/// </summary>
public static class AdminActivateRoleMetaField
{
    public static readonly RouteMetadata AdminActivateRole = new(
        "AdminActivateRole",
        "Activate a role",
        """
            Activates a role, allowing it to be assigned to users.
            \n
            This endpoint activates a role by:\n
            - Validating the role ID exists\n
            - Checking that the role is not already active\n
            - Setting the role's IsActive status to true\n
            - Returning the updated role details\n
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Use Cases:**\n
            - Re-enable a previously deactivated role\n
            - Make a role available for assignment to users\n
            \n
            **Response Includes:**\n
            - Role ID, name, and description\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with activated role details on success\n
            - Returns 400 Bad Request if validation fails\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 404 Not Found if role doesn't exist\n
            - Returns 409 Conflict if role is already active\n
            \n
            **Error Handling:**\n
            - ValidationException (400): Invalid role ID format\n
            - NotFoundException (404): Role not found with the specified ID\n
            - ConflictException (409): Role is already active
        """
    );
}
