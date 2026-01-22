namespace _116.Identity.Application.Roles.Constants;

/// <summary>
/// Constants for permission-related API routes.
/// </summary>
public static class PermissionRouteConstants
{
    /// <summary>
    /// Base endpoint for permission operations.
    /// </summary>
    public const string Endpoint = "permissions";

    /// <summary>
    /// Route segment for activating a permission.
    /// </summary>
    public const string Activate = "activate";

    /// <summary>
    /// Route segment for deactivating a permission.
    /// </summary>
    public const string Deactivate = "deactivate";

    /// <summary>
    /// Route segment for restoring a soft-deleted permission.
    /// </summary>
    public const string Restore = "restore";

    /// <summary>
    /// Route segment for hard deleting a permission.
    /// </summary>
    public const string Hard = "hard";
}
