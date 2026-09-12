using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace _116.Core.Infrastructure.Services;

/// <summary>
/// The resilience policy every cloud-storage call runs through.
/// </summary>
/// <remarks>
/// Every value here is set explicitly. Polly's defaults are tuned for chatty HTTP traffic — the
/// breaker alone needs 100 calls inside its sampling window before it will ever open — and an
/// upload workload never reaches that volume, so a defaulted breaker would be inert.
/// </remarks>
public static class CloudStorageResilience
{
    /// <summary>
    /// How long one provider attempt may take before it is abandoned. Matches the timeout the
    /// other outbound clients in this codebase use.
    /// </summary>
    public static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

    /// <summary>
    /// How many times a failed call is retried before the failure surfaces.
    /// </summary>
    public const int MaxRetryAttempts = 3;

    /// <summary>
    /// The first retry delay; later attempts back off exponentially from it. Kept well under
    /// <see cref="Timeout" /> so a retried upload still answers inside a sane request budget.
    /// </summary>
    public static readonly TimeSpan RetryDelay = TimeSpan.FromMilliseconds(500);

    /// <summary>
    /// How many calls the breaker needs inside its sampling window before it will act.
    /// </summary>
    public const int MinimumThroughput = 10;

    /// <summary>
    /// The share of calls that must fail, once <see cref="MinimumThroughput" /> is met, to open
    /// the breaker.
    /// </summary>
    public const double FailureRatio = 0.5;

    /// <summary>
    /// The window failures are counted over.
    /// </summary>
    public static readonly TimeSpan SamplingDuration = TimeSpan.FromSeconds(30);

    /// <summary>
    /// How long the breaker stays open before letting a trial call through.
    /// </summary>
    public static readonly TimeSpan BreakDuration = TimeSpan.FromSeconds(15);

    /// <summary>
    /// The retry policy, exposed so tests assert the shipped configuration rather than a copy.
    /// </summary>
    /// <returns>The retry options.</returns>
    public static RetryStrategyOptions CreateRetryOptions() =>
        new()
        {
            MaxRetryAttempts = MaxRetryAttempts,
            Delay = RetryDelay,
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true,
        };

    /// <summary>
    /// The breaker policy, exposed so tests assert the shipped configuration rather than a copy.
    /// </summary>
    /// <returns>The circuit-breaker options.</returns>
    public static CircuitBreakerStrategyOptions CreateCircuitBreakerOptions() =>
        new()
        {
            FailureRatio = FailureRatio,
            MinimumThroughput = MinimumThroughput,
            SamplingDuration = SamplingDuration,
            BreakDuration = BreakDuration,
        };

    /// <summary>
    /// Builds the pipeline: jittered exponential retries, then a breaker so a provider outage
    /// fails fast instead of queueing uploads, then a per-attempt time limit.
    /// </summary>
    /// <returns>The pipeline.</returns>
    /// <remarks>
    /// Register the result once per process. A per-scope pipeline would rebuild the breaker's
    /// state on every request and could therefore never open.
    /// </remarks>
    public static ResiliencePipeline CreatePipeline()
    {
        return new ResiliencePipelineBuilder()
            .AddRetry(CreateRetryOptions())
            .AddCircuitBreaker(CreateCircuitBreakerOptions())
            .AddTimeout(Timeout)
            .Build();
    }
}
