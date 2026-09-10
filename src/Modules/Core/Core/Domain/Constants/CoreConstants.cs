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

    /// <summary>
    /// How long an unclaimed upload is kept before the reaper removes it and its remote asset.
    /// Sized well above the seconds a referencing write needs, so a slow request is never reaped
    /// out from under itself.
    /// </summary>
    public static readonly TimeSpan UnclaimedFileGracePeriod = TimeSpan.FromHours(24);

    /// <summary>
    /// Maximum unclaimed uploads removed per reaper run.
    /// </summary>
    public const int UnclaimedFileReapBatchSize = 100;

    /// <summary>
    /// Cron expression for the unclaimed-upload reaper: hourly, on the hour.
    /// </summary>
    public const string UnclaimedFileReapCron = "0 0 * * * ?";
}
