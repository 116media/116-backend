namespace _116.Shared.Application.Builders.RateLimit;

/// <summary>
/// A per-account rate limiter for the pre-auth security endpoints, keyed by a stable account
/// identifier (normalized email) rather than by caller IP. Complements the middleware limiter so a
/// single account cannot be brute-forced from many IPs. Throws <c>RateLimitExceededException</c> when
/// the account's window is exhausted, matching the middleware's 429 contract.
/// </summary>
public interface IAccountRateLimiter
{
    /// <summary>
    /// Consumes one permit for <paramref name="accountKey" /> under <paramref name="policyName" />,
    /// throwing when the account has exceeded that policy's window. Policies without a per-account
    /// limiter, and blank account keys, are a no-op.
    /// </summary>
    /// <param name="policyName">The rate-limit policy to charge against.</param>
    /// <param name="accountKey">The account identifier (typically the email); normalized internally.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    Task EnsureWithinLimitAsync(string policyName, string accountKey, CancellationToken cancellationToken);
}
