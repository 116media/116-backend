namespace _116.Identity.Application.Roles.Constants;

/// <summary>
/// Contains route path constants for role-related API endpoints.
/// Provides centralized string constants for URL segments used in role management routes.
/// </summary>
public static class RoleRouteConstants
{
    /// <summary>
    /// The base endpoint path for all role routes.
    /// Combined with admin prefix: /api/v1/admin/roles.
    /// </summary>
    public const string Endpoint = "roles";

    /// <summary>
    /// Route segment for activating a role.
    /// Example: /api/v1/admin/roles/{id}/activate.
    /// </summary>
    public const string Activate = "activate";

    /// <summary>
    /// Route segment for deactivating a role.
    /// Example: /api/v1/admin/roles/{id}/deactivate.
    /// </summary>
    public const string Deactivate = "deactivate";

    /// <summary>
    /// Route segment for restoring a soft-deleted role.
    /// Example: /api/v1/admin/roles/{id}/restore.
    /// </summary>
    public const string Restore = "restore";

    /// <summary>
    /// Route segment for hard deleting a role.
    /// Example: /api/v1/admin/roles/{id}/hard.
    /// </summary>
    public const string Hard = "hard";

    /// <summary>
    /// Route segment for role permissions.
    /// Example: /api/v1/admin/roles/{id}/permissions.
    /// </summary>
    public const string Permissions = "permissions";
}
