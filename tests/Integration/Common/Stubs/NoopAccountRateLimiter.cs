using _116.Shared.Application.Builders.RateLimit;

namespace _116.Integration.Tests.Common.Stubs;

/// <summary>
/// No-op <see cref="IAccountRateLimiter" /> for the general test host, so per-account throttling never
/// rejects tests that reuse an account within a window. The dedicated rate-limited host keeps the real
/// limiter, and pre-auth endpoints exercised there consume permits at the middleware before a handler
/// runs, so this stub does not weaken those assertions.
/// </summary>
public sealed class NoopAccountRateLimiter : IAccountRateLimiter
{
    /// <inheritdoc />
    public Task EnsureWithinLimitAsync(string policyName, string accountKey, CancellationToken cancellationToken) =>
        Task.CompletedTask;
}
