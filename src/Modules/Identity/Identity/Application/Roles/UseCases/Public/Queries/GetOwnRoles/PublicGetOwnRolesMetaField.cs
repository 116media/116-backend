using _116.Shared.Application.Metadata;

namespace _116.Identity.Application.Roles.UseCases.Public.Queries.GetOwnRoles;

/// <summary>
/// Contains metadata information for the public get own roles route.
/// </summary>
public static class PublicGetOwnRolesMetaField
{
    /// <summary>
    /// Metadata describing the get own roles endpoint.
    /// </summary>
    public static readonly RouteMetadata GetOwnRoles = new(
        "PublicGetOwnRoles",
        "Retrieve the authenticated user's roles and permissions",
        """
            Retrieves all roles assigned to the currently authenticated user, each including
            its full set of permissions.\n
            \n
            This endpoint is intended for client applications that need to determine what
            features and actions the current user is allowed to perform, enabling role-based
            UI rendering and frontend access control.\n
            \n
            **Authentication Requirements:**\n
            - Valid JWT Bearer token required\n
            - Only visitor role users can access this endpoint\n
            \n
            **Returned Information:**\n
            - List of roles assigned to the user\n
            - Each role includes its name, description, active status, and full permission list\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with the user's roles and permissions\n
            - Returns 401 Unauthorized for invalid or missing JWT token\n
            - Returns 403 Forbidden for inactive or unverified accounts\n
            - Returns 404 Not Found if the user no longer exists\n
        """
    );
}
