using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace _116.Shared.Application.Builders.RateLimit;

/// <summary>
/// Builder for configuring Fixed Window rate limiting policies.
/// <para>
/// Fixed Window rate limiting is best suited for general-access endpoints
/// with predictable traffic patterns, such as content browsing,
/// user profile access, and read-heavy operations.
/// </para>
/// </summary>
public class FixedWindowBuilder(RateLimiterOptions options)
{
    /// <summary>
    /// Adds a Fixed Window rate limiting policy with the specified configuration.
    /// </summary>
    /// <param name="policyName">The name of the rate limit policy.</param>
    /// <param name="permitLimit">Maximum number of permits allowed per window.</param>
    /// <param name="windowSeconds">The duration of the fixed window in seconds.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public FixedWindowBuilder AddPolicy(string policyName, int permitLimit, int windowSeconds)
    {
        options.AddPolicy(
            policyName,
            httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: RateLimitPartitioning.ResolvePartitionKey(httpContext),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = permitLimit,
                        Window = TimeSpan.FromSeconds(windowSeconds),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0,
                    }
                )
        );

        return this;
    }
}
