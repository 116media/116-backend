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
    /// Whether to enable database migrations for this module.
    /// Default is true.
    /// </summary>
    public bool EnableMigrations { get; init; } = true;

    /// <summary>
    /// Whether to enable data seeding for this module.
    /// Default is true.
    /// </summary>
    public bool EnableSeeding { get; init; } = true;

    /// <summary>
    /// Whether to use connection pooling for the DbContext.
    /// Default is true.
    /// </summary>
    public bool UseConnectionPooling { get; init; } = true;
}
