namespace _116.Auth.Domain.Constants;

/// <summary>
/// Contains constant values for the Auth module.
/// Provides centralized string constants for module identification, route prefixes,
/// and database schema naming to ensure consistency across the application.
/// </summary>
public static class AuthConstants
{
    /// <summary>
    /// Database schema name for authentication-related tables.
    /// Used in Entity Framework configurations to organize auth tables under the "authentication" schema.
    /// </summary>
    public const string SchemaName = "authentication";

    /// <summary>
    /// Identifies the Auth module within the application.
    /// Used for module registration and configuration.
    /// </summary>
    public const string ModuleName = "Auth";

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
}
