[assembly: AssemblyFixture(typeof(TestContainersFixture))]

namespace _116.Integration.Tests.Common.Fixtures;

/// <summary>
/// Ties the lifetime of the assembly-wide PostgreSQL and Redis containers to the test run, so
/// they outlive every collection that uses them and are removed once they have all finished.
/// </summary>
public class TestContainersFixture : IAsyncLifetime
{
    /// <inheritdoc />
    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await TestPostgresContainer.ShutdownAsync();
        await TestRedisContainer.ShutdownAsync();
    }
}
