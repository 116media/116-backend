namespace _116.Integration.Tests.Common.Fixtures;

/// <summary>
/// A <see cref="PostgresFixture" /> dedicated to the rate limiting test collection.
/// Runs its own Testcontainer and its own application host so that permits consumed while
/// driving a policy to rejection can never leak into the shared "Database" collection.
/// </summary>
public class RateLimitedPostgresFixture : PostgresFixture
{
    /// <inheritdoc />
    protected override ApiFixture CreateApiFixture() => new RateLimitedApiFixture(this);
}
