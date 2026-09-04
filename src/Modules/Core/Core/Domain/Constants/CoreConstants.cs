namespace _116.Core.Domain.Constants;

/// <summary>
/// Contains constant values for the Core module.
/// Provides centralized string constants for module identification and database schema naming
/// to ensure consistency across the application.
/// </summary>
public static class CoreConstants
{
    /// <summary>
    /// Database schema name for core-related tables.
    /// Used in Entity Framework configurations to organize core tables under the "core" schema.
    /// </summary>
    public const string SchemaName = "core";

    /// <summary>
    /// Identifies the Core module within the application.
    /// Used for module registration and configuration.
    /// </summary>
    public const string ModuleName = "Core";
}
