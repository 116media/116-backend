[assembly: AssemblyFixture(typeof(PostgresContainerFixture))]

namespace _116.Integration.Tests.Common.Fixtures;

/// <summary>
/// Ties the lifetime of the assembly-wide PostgreSQL container to the test run, so the container
/// outlives every collection that leases a database from it and is removed once they have all
/// finished.
/// </summary>
public class PostgresContainerFixture : IAsyncLifetime
{
    /// <inheritdoc />
    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    /// <inheritdoc />
    public ValueTask DisposeAsync() => TestPostgresContainer.ShutdownAsync();
}
