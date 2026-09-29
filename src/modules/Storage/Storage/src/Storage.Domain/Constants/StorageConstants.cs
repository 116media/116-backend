namespace _116.Storage.Domain.Constants;

/// <summary>
/// Contains constant values for the Storage module.
/// Provides centralized string constants for module identification and database schema naming
/// to ensure consistency across the application.
/// </summary>
public static class StorageConstants
{
    /// <summary>
    /// Database schema name for core-related tables.
    /// Used in Entity Framework configurations to organize core tables under the "storage" schema.
    /// </summary>
    public const string SchemaName = "storage";

    /// <summary>
    /// Identifies the Storage module within the application.
    /// Used for module registration and configuration.
    /// </summary>
    public const string ModuleName = "Storage";
}
