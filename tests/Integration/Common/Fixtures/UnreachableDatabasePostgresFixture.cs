namespace _116.Integration.Tests.Common.Fixtures;

/// <summary>
/// A <see cref="PostgresFixture" /> dedicated to the degraded-readiness test collection. It
/// leases its own database and boots its own host, so the misdirected port is never observed by
/// the "Database" collection.
/// </summary>
public class UnreachableDatabasePostgresFixture : PostgresFixture
{
    /// <inheritdoc />
    protected override ApiFixture CreateApiFixture() => new UnreachableDatabaseApiFixture(this);
}
