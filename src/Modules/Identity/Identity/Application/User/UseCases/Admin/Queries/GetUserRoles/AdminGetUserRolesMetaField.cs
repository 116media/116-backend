using _116.Shared.Application.Metadata;

namespace _116.Identity.Application.User.UseCases.Admin.Queries.GetUserRoles;

/// <summary>
/// Contains metadata information for the admin get user roles route.
/// </summary>
public static class AdminGetUserRolesMetaField
{
    public static readonly RouteMetadata AdminGetUserRoles = new(
        "AdminGetUserRoles",
        "Get user's roles",
        """
            Retrieves all roles assigned to a specific user.
            \n
            This endpoint retrieves user roles by:\n
            - Validating the user ID\n
            - Fetching all role assignments for the user\n
            - Returning the list of roles with details\n
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Use Cases:**\n
            - View user's current role assignments\n
            - Audit user permissions\n
            \n
            **Response Includes:**\n
            - List of roles with ID, name, and description\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with list of roles on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks required role\n
            \n
            **Error Handling:**\n
            - ValidationException (400): Invalid user ID format
        """
    );
}
