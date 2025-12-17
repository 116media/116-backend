using System.Reflection;

using _116.Core.Domain.Constants;
using _116.Core.Domain.Entities;

using Microsoft.EntityFrameworkCore;

namespace _116.Core.Infrastructure.Persistence;

/// <summary>
/// Entity Framework database context for the core module.
/// Manages file entities and related core data within the "core" schema.
/// </summary>
/// <param name="options">The database context configuration options</param>
/// <remarks>
/// This context provides access to system-wide data including file management.
/// All entities are stored in the "core" schema.
/// </remarks>
public class CoreDbContext(DbContextOptions<CoreDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Gets the collection of file entities representing uploaded files in the system.
    /// </summary>
    /// <value>DbSet of FileEntity for managing file metadata</value>
    public DbSet<FileEntity> Files => Set<FileEntity>();

    /// <summary>
    /// Configures the model for the context using Fluent API.
    /// </summary>
    /// <param name="modelBuilder">The builder used to construct the model for the context.</param>
    /// <remarks>
    /// Sets the default schema to "system" and applies all entity configurations from the current assembly.
    /// </remarks>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(CoreConstants.SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
