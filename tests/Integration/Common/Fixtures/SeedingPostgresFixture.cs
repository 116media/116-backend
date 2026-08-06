namespace _116.Integration.Tests.Common.Fixtures;

/// <summary>
/// A <see cref="PostgresFixture" /> dedicated to the seeding tests. It leases its own database
/// from the shared container and boots its own application host, so the rows the module seeders
/// write at startup survive for the host's lifetime and are visible to no other collection.
/// </summary>
public class SeedingPostgresFixture : PostgresFixture
{
    /// <inheritdoc />
    protected override ApiFixture CreateApiFixture() => new SeedingApiFixture(this);
}
