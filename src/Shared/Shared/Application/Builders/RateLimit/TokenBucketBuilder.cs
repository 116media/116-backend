using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace _116.Shared.Application.Builders.RateLimit;

/// <summary>
/// Builder for configuring Token Bucket rate limiting policies.
/// <para>
/// Token Bucket rate limiting is best suited for resource-intensive endpoints
/// where short bursts of requests are acceptable, but sustained usage
/// must be controlled, such as file uploads and data exports.
/// </para>
/// </summary>
public class TokenBucketBuilder(RateLimiterOptions options)
{
    /// <summary>
    /// Adds a Token Bucket rate limiting policy with the specified configuration.
    /// </summary>
    /// <param name="policyName">The name of the rate limit policy.</param>
    /// <param name="tokenLimit">Maximum number of tokens in the bucket.</param>
    /// <param name="tokensPerPeriod">Number of tokens replenished per period.</param>
    /// <param name="replenishmentPeriodSeconds">Replenishment period duration in seconds.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public TokenBucketBuilder AddPolicy(
        string policyName,
        int tokenLimit,
        int tokensPerPeriod,
        int replenishmentPeriodSeconds
    )
    {
        options.AddTokenBucketLimiter(
            policyName,
            limiterOptions =>
            {
                limiterOptions.TokenLimit = tokenLimit;
                limiterOptions.TokensPerPeriod = tokensPerPeriod;
                limiterOptions.ReplenishmentPeriod = TimeSpan.FromSeconds(replenishmentPeriodSeconds);
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 0;
            }
        );

        return this;
    }
}
