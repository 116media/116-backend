using _116.Identity.Domain.Enums;
using _116.Shared.Application.Configurations;

namespace _116.Identity.Infrastructure.Persistence.Seeds.SuperAdmin;

/// <summary>
/// Configuration class for Super Admin seeding operations.
/// Centralizes all Super Admin related constants and settings.
/// </summary>
public static class SuperAdminConfiguration
{
    /// <summary>
    /// Super Admin email address.
    /// </summary>
    public const string Email = "superadmin@116.com";
    /// <summary>
    /// Super Admin username.
    /// </summary>
    public const string Username = "sigmacool";
    /// <summary>
    /// Super Admin role name derived from CoreUserRole enum.
    /// </summary>
    public static string RoleName => nameof(EnumCoreUserRole.SuperAdmin);
    /// <summary>
    /// Super Admin role description.
    /// </summary>
    public const string RoleDescription = "Super Administrator with complete system access and control";
    /// <summary>
    /// System-wide permission resource name.
    /// </summary>
    public const string PermissionResource = "system";
    /// <summary>
    /// System-wide permission action name.
    /// </summary>
    public const string PermissionAction = "all";
    /// <summary>
    /// System-wide permission description.
    /// </summary>
    public const string PermissionDescription = "Complete system access - grants all permissions for all resources";
    /// <summary>
    /// Gets the Super Admin password from environment configuration.
    /// Falls back to a default secure password if not configured.
    /// </summary>
    /// <returns>The Super Admin password.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no password is configured.</exception>
    public static string GetPassword()
    {
        string? password = AppEnvironment.DefaultPassword();
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException("DEFAULT_USER_PASSWORD env variable is missing or empty.");
        }
        return password;
    }
}
