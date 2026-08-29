namespace _116.Integration.Tests.Common.Fixtures;

/// <summary>
/// An <see cref="ApiFixture" /> that keeps the real per-account throttle while leaving the middleware
/// rate limiter disabled. This isolates <c>IAccountRateLimiter</c> so a test can prove per-account
/// throttling without the middleware's per-IP limiter rejecting first at the same threshold.
/// </summary>
/// <remarks>
/// The account limiter is a single host-wide instance whose permits are never restored between tests,
/// so tests on this host must use unique account keys to stay isolated. Never share this host with the
/// general suite.
/// </remarks>
/// <param name="db">The Testcontainer database backing this host.</param>
public class AccountRateLimitedApiFixture(PostgresFixture db) : ApiFixture(db)
{
    /// <inheritdoc />
    protected override bool DisableAccountRateLimiter => false;
}
