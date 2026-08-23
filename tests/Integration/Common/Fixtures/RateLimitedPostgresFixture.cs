namespace _116.Integration.Tests.Common.Fixtures;

/// <summary>
/// A <see cref="PostgresFixture" /> dedicated to the rate limiting test collection.
/// It leases its own database from the shared container and boots its own application host, so
/// permits consumed while driving a policy to rejection can never leak into the "Database"
/// collection.
/// </summary>
public class RateLimitedPostgresFixture : PostgresFixture
{
    /// <inheritdoc />
    protected override ApiFixture CreateApiFixture() => new RateLimitedApiFixture(this);
}
