namespace _116.Integration.Tests.Common.Fixtures;

/// <summary>
/// A <see cref="PostgresFixture" /> dedicated to the Resend-deployment test collection. It leases
/// its own database and boots its own host, so the swapped provider is never observed by the
/// "Database" collection.
/// </summary>
public class ResendPostgresFixture : PostgresFixture
{
    /// <inheritdoc />
    protected override ApiFixture CreateApiFixture() => new ResendApiFixture(this);
}
