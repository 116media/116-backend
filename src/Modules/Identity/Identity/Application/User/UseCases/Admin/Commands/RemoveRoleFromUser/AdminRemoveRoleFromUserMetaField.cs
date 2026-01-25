using _116.Shared.Application.Metadata;

namespace _116.Identity.Application.User.UseCases.Admin.Commands.RemoveRoleFromUser;

/// <summary>
/// Contains metadata information for the admin remove role from user route.
/// </summary>
public static class AdminRemoveRoleFromUserMetaField
{
    public static readonly RouteMetadata AdminRemoveRoleFromUser = new(
        "AdminRemoveRoleFromUser",
        "Remove a role from a user",
        """
            Removes a role from a user, revoking all permissions associated with that role.
            \n
            This endpoint removes a role by:\n
            - Checking that the role is assigned to the user\n
            - Removing the user-role association\n
            - Returning the user's updated roles\n
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Use Cases:**\n
            - Revoke capabilities from a user\n
            - Demote user from a role\n
            \n
            **Response Includes:**\n
            - List of remaining roles assigned to the user\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with updated roles on success\n
            - Returns 400 Bad Request if role is not assigned to user\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            \n
            **Error Handling:**\n
            - ValidationException (400): Invalid user ID or role ID format\n
            - BadRequestException (400): Role is not assigned to the user
        """
    );
}
