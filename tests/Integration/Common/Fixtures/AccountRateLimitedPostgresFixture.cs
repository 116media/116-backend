namespace _116.Integration.Tests.Common.Fixtures;

/// <summary>
/// A <see cref="PostgresFixture" /> dedicated to the account-throttle test collection. It leases its
/// own database and boots its own application host, so permits consumed by the per-account throttle
/// never leak into other collections.
/// </summary>
public class AccountRateLimitedPostgresFixture : PostgresFixture
{
    /// <inheritdoc />
    protected override ApiFixture CreateApiFixture() => new AccountRateLimitedApiFixture(this);
}
