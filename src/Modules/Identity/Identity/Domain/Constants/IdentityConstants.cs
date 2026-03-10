namespace _116.Identity.Domain.Constants;

/// <summary>
/// Contains constant values for the Identity module.
/// Provides centralized string constants for module identification, route prefixes,
/// and database schema naming to ensure consistency across the application.
/// </summary>
public static class IdentityConstants
{
    /// <summary>
    /// Database schema name for identity-related tables.
    /// Used in Entity Framework configurations to organize identity tables under the "authentication" schema.
    /// </summary>
    public const string SchemaName = "identity";

    /// <summary>
    /// Identifies the Identity module within the application.
    /// Used for module registration and configuration.
    /// </summary>
    public const string ModuleName = "Identity";

    /// <summary>
    /// Route prefix for administrative authentication endpoints.
    /// Used to construct admin-specific API routes (e.g., /api/v1/admin/auth).
    /// </summary>
    public const string Admin = "admin";

    /// <summary>
    /// Route prefix for public authentication endpoints.
    /// Used to construct publicly accessible API routes (e.g., /api/v1/public/auth).
    /// </summary>
    public const string Public = "public";

    /// <summary>
    /// Route segment for self-referencing endpoints scoped to the authenticated user.
    /// Used to construct "own resource" API routes (e.g., /api/v1/public/me/profile).
    /// </summary>
    public const string Me = "me";
}
