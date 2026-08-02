namespace _116.Integration.Tests.Common.Fixtures;

/// <summary>
/// An <see cref="ApiFixture" /> that boots the application with the production rate limit
/// policies left intact, so that <c>RateLimitingExtension.AddRateLimiting</c> and its sliding
/// window, token bucket, and fixed window configuration run for real.
/// </summary>
/// <remarks>
/// Every other aspect of the host — Testcontainer database, JWT overrides, external service
/// stubs — is inherited unchanged from <see cref="ApiFixture" />.
/// Requests issued against this host consume real permits, so it must never be shared with the
/// general test suite.
/// </remarks>
/// <param name="db">The Testcontainer database backing this host.</param>
public class RateLimitedApiFixture(PostgresFixture db) : ApiFixture(db)
{
    /// <inheritdoc />
    protected override bool DisableRateLimits => false;
}
