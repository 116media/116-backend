using System.Threading.RateLimiting;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Shared.Application.Exceptions;

namespace _116.Shared.Application.Builders.RateLimit;

/// <summary>
/// In-process <see cref="IAccountRateLimiter" />. Holds one sliding-window limiter per pre-auth
/// security policy, partitioned by normalized account key. Registered as a singleton so the windows
/// persist across requests for the process lifetime. A multi-instance deployment needs a distributed
/// store to share these windows across nodes.
/// </summary>
public sealed class AccountRateLimiter : IAccountRateLimiter, IAsyncDisposable
{
    private readonly IReadOnlyDictionary<string, PartitionedRateLimiter<string>> _limiters;

    /// <summary>
    /// Builds the per-account limiters from the same policy constants as the middleware limiters.
    /// </summary>
    public AccountRateLimiter()
    {
        _limiters = new Dictionary<string, PartitionedRateLimiter<string>>
        {
            [RateLimitPolicies.Authentication] = BuildSliding(
                AuthenticationRateLimitConstants.PermitLimit,
                AuthenticationRateLimitConstants.WindowSeconds,
                AuthenticationRateLimitConstants.SegmentsPerWindow
            ),
            [RateLimitPolicies.Otp] = BuildSliding(
                OtpRateLimitConstants.PermitLimit,
                OtpRateLimitConstants.WindowSeconds,
                OtpRateLimitConstants.SegmentsPerWindow
            ),
            [RateLimitPolicies.PasswordManagement] = BuildSliding(
                PasswordManagementRateLimitConstants.PermitLimit,
                PasswordManagementRateLimitConstants.WindowSeconds,
                PasswordManagementRateLimitConstants.SegmentsPerWindow
            ),
        };
    }

    /// <inheritdoc />
    public async Task EnsureWithinLimitAsync(string policyName, string accountKey, CancellationToken cancellationToken)
    {
        if (
            string.IsNullOrWhiteSpace(accountKey)
            || !_limiters.TryGetValue(policyName, out PartitionedRateLimiter<string>? limiter)
        )
        {
            return;
        }

        string key = accountKey.Trim().ToLowerInvariant();

        using RateLimitLease lease = await limiter.AcquireAsync(key, permitCount: 1, cancellationToken);
        if (lease.IsAcquired)
        {
            return;
        }

        TimeSpan retryAfter = lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retry) ? retry : TimeSpan.Zero;
        throw new RateLimitExceededException(retryAfter);
    }

    private static PartitionedRateLimiter<string> BuildSliding(
        int permitLimit,
        int windowSeconds,
        int segmentsPerWindow
    ) =>
        PartitionedRateLimiter.Create<string, string>(key =>
            RateLimitPartition.GetSlidingWindowLimiter(
                key,
                _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = permitLimit,
                    Window = TimeSpan.FromSeconds(windowSeconds),
                    SegmentsPerWindow = segmentsPerWindow,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0,
                }
            )
        );

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        foreach (PartitionedRateLimiter<string> limiter in _limiters.Values)
        {
            await limiter.DisposeAsync();
        }
    }
}
