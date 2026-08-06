namespace _116.Integration.Tests.Common.Fixtures;

/// <summary>
/// A <see cref="PostgresFixture" /> dedicated to the CORS test collection. It leases its own
/// database from the shared container and boots its own application host, so the restricted
/// default policy that host is built with is never observed by the "Database" collection.
/// </summary>
public class CorsPostgresFixture : PostgresFixture
{
    /// <inheritdoc />
    protected override ApiFixture CreateApiFixture() => new CorsApiFixture(this);
}
