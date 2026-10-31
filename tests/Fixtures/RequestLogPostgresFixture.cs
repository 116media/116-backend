namespace _116.Tests.Fixtures;

/// <summary>
/// A <see cref="PostgresFixture" /> dedicated to the request-logging test collection. It leases
/// its own database and boots its own host, so the replaced logger is never observed by the
/// "Database" collection.
/// </summary>
public class RequestLogPostgresFixture : PostgresFixture
{
    /// <inheritdoc />
    protected override ApiFixture CreateApiFixture() => new RequestLogApiFixture(this);
}
