using Microsoft.EntityFrameworkCore;

namespace _116.Shared.Infrastructure;

/// <summary>
/// Configuration options for module registration and setup.
/// </summary>
public class ModuleOptions<TDbContext>
    where TDbContext : DbContext
{
    /// <summary>
    /// The name of the module (e.g., "User", "Basket").
    /// </summary>
    public required string ModuleName { get; init; }

    /// <summary>
    /// The database schema name for this module.
    /// If null, use the module name in lowercase.
    /// </summary>
    public string? SchemaName { get; init; }

    /// <summary>
    /// Whether to use connection pooling for the DbContext.
    /// Default is true.
    /// </summary>
    public bool UseConnectionPooling { get; init; } = true;

    /// <summary>
    /// Whether queries default to no-tracking for this module's DbContext.
    /// Write-path repository methods opt back in with AsTracking. Default is false.
    /// </summary>
    public bool UseNoTrackingByDefault { get; init; }
}
