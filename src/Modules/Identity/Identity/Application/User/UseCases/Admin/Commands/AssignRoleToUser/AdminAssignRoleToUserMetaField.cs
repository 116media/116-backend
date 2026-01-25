using _116.Shared.Application.Metadata;

namespace _116.Identity.Application.User.UseCases.Admin.Commands.AssignRoleToUser;

/// <summary>
/// Contains metadata information for the admin assign role to user route.
/// </summary>
public static class AdminAssignRoleToUserMetaField
{
    public static readonly RouteMetadata AdminAssignRoleToUser = new(
        "AdminAssignRoleToUser",
        "Assign a role to a user",
        """
            Assigns a role to a user, granting them all permissions associated with that role.
            \n
            This endpoint assigns a role by:\n
            - Validating the role ID exists and is active\n
            - Checking that the role is not already assigned to the user\n
            - Creating the user-role association\n
            - Returning the user's updated roles\n
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Use Cases:**\n
            - Grant new capabilities to a user\n
            - Promote user to a higher role\n
            \n
            **Request Body:**\n
            - roleId: The ID of the role to assign\n
            \n
            **Response Includes:**\n
            - List of all roles assigned to the user\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with updated roles on success\n
            - Returns 400 Bad Request if role is inactive or deleted\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 404 Not Found if role doesn't exist\n
            - Returns 409 Conflict if role is already assigned to user\n
            \n
            **Error Handling:**\n
            - ValidationException (400): Invalid user ID or role ID format\n
            - BadRequestException (400): Role is inactive or deleted\n
            - NotFoundException (404): Role not found\n
            - ConflictException (409): Role is already assigned to the user
        """
    );
}
