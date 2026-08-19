using Microsoft.EntityFrameworkCore.Migrations;

namespace _116.Unit.Tests.Common;

/// <summary>
/// Migrator replacing the relational one on a test <c>DbContext</c> so the
/// startup migration step a module runs completes without a database. Register
/// it through <c>DbContextOptionsBuilder.ReplaceService</c> to exercise the
/// pipeline code that follows the migration call.
/// </summary>
public sealed class NoOpMigrator : IMigrator
{
    /// <inheritdoc />
    public void Migrate(string? targetMigration = null) { }

    /// <inheritdoc />
    public Task MigrateAsync(string? targetMigration = null, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public string GenerateScript(
        string? fromMigration = null,
        string? toMigration = null,
        MigrationsSqlGenerationOptions options = MigrationsSqlGenerationOptions.Default
    )
    {
        return string.Empty;
    }

    /// <inheritdoc />
    public bool HasPendingModelChanges()
    {
        return false;
    }
}
