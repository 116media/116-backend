using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace _116.Shared.Application.Builders.RateLimit;

/// <summary>
/// Builder for configuring Sliding Window rate limiting policies.
/// <para>
/// Sliding Window rate limiting is best suited for security-critical endpoints
/// where request distribution over time matters, such as authentication,
/// OTP verification, and password management.
/// </para>
/// </summary>
public class SlidingWindowBuilder(RateLimiterOptions options)
{
    /// <summary>
    /// Adds a Sliding Window rate limiting policy with the specified configuration.
    /// </summary>
    /// <param name="policyName">The name of the rate limit policy.</param>
    /// <param name="permitLimit">Maximum number of permits allowed within the window.</param>
    /// <param name="windowSeconds">The duration of the sliding window in seconds.</param>
    /// <param name="segmentsPerWindow">Number of segments to divide the window into for sliding behavior.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public SlidingWindowBuilder AddPolicy(string policyName, int permitLimit, int windowSeconds, int segmentsPerWindow)
    {
        options.AddSlidingWindowLimiter(
            policyName,
            limiterOptions =>
            {
                limiterOptions.PermitLimit = permitLimit;
                limiterOptions.Window = TimeSpan.FromSeconds(windowSeconds);
                limiterOptions.SegmentsPerWindow = segmentsPerWindow;
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 0;
            }
        );
        return this;
    }
}
