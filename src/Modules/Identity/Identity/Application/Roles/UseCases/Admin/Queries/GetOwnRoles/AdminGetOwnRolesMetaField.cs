using _116.Shared.Application.Metadata;

namespace _116.Identity.Application.Roles.UseCases.Admin.Queries.GetOwnRoles;

/// <summary>
/// Contains metadata information for the admin get own roles route.
/// </summary>
public static class AdminGetOwnRolesMetaField
{
    /// <summary>
    /// Metadata describing the admin get own roles endpoint.
    /// </summary>
    public static readonly RouteMetadata GetOwnRoles = new(
        "AdminGetOwnRoles",
        "Retrieve the authenticated admin's roles and permissions",
        """
            Retrieves all roles assigned to the currently authenticated admin user, each including
            its full set of permissions.\n
            \n
            This lightweight endpoint is intended for client applications that need to check or
            refresh the current admin's permissions without fetching the full profile, enabling
            role-based UI rendering and frontend access control.\n
            \n
            **Authentication Requirements:**\n
            - Valid JWT Bearer token required\n
            - Admin or SuperAdmin role required\n
            \n
            **Returned Information:**\n
            - List of roles assigned to the admin\n
            - Each role includes its name, description, active status, and full permission list\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with the admin's roles and permissions\n
            - Returns 401 Unauthorized for invalid or missing JWT token\n
            - Returns 403 Forbidden for insufficient permissions\n
            - Returns 404 Not Found if the user no longer exists\n
        """
    );
}
