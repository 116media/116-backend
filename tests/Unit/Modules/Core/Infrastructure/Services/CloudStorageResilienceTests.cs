using _116.Core.Infrastructure.Services;
using AwesomeAssertions;
using Polly;
using Polly.CircuitBreaker;
using Polly.Timeout;
using Xunit;

namespace _116.Unit.Tests.Modules.Core.Infrastructure.Services;

/// <summary>
/// Unit tests for the cloud-storage resilience policy. These assert the policy itself rather than
/// any provider call, because the policy is what decides whether a storage blip costs one failed
/// upload or a cascade. The breaker is exercised on its own options so the retry backoff does not
/// make the test sleep for minutes.
/// </summary>
public class CloudStorageResilienceTests
{
    [Fact]
    public async Task RetryPolicy_WhenACallFailsTransiently_ShouldRetryBeforeSurfacingTheFailure()
    {
        ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
            .AddRetry(CloudStorageResilience.CreateRetryOptions())
            .Build();
        var attempts = 0;

        Func<Task> act = async () =>
            await pipeline.ExecuteAsync(_ =>
            {
                attempts++;
                throw new HttpRequestException("cloudinary unreachable");
            });

        await act.Should().ThrowAsync<HttpRequestException>();

        // The first call plus every retry: a single blip must not become a failed upload.
        attempts.Should().Be(CloudStorageResilience.MaxRetryAttempts + 1);
    }

    [Fact]
    public async Task Pipeline_WhenACallSucceeds_ShouldNotRetry()
    {
        ResiliencePipeline pipeline = CloudStorageResilience.CreatePipeline();
        var attempts = 0;

        await pipeline.ExecuteAsync(_ =>
        {
            attempts++;
            return ValueTask.CompletedTask;
        });

        attempts.Should().Be(1);
    }

    [Fact]
    public async Task Breaker_WhenFailuresPersist_ShouldOpenAndStopCallingTheProvider()
    {
        ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
            .AddCircuitBreaker(CloudStorageResilience.CreateCircuitBreakerOptions())
            .Build();
        var attempts = 0;

        async Task Call() =>
            await pipeline.ExecuteAsync(_ =>
            {
                attempts++;
                throw new HttpRequestException("cloudinary down");
            });

        // Enough sustained failure to satisfy the breaker's minimum throughput.
        for (var round = 0; round < CloudStorageResilience.MinimumThroughput; round++)
        {
            try
            {
                await Call();
            }
            catch (Exception exception) when (exception is HttpRequestException or BrokenCircuitException)
            {
                // Both outcomes are expected while the breaker is closing.
            }
        }

        int attemptsBeforeOpen = attempts;

        Func<Task> act = Call;
        await act.Should().ThrowAsync<BrokenCircuitException>();

        // An open breaker fails fast: the provider is not called again.
        attempts.Should().Be(attemptsBeforeOpen);
    }

    [Fact]
    public void Breaker_ShouldNotRelyOnPollysDefaultThroughput()
    {
        // Polly defaults to 100 calls per sampling window, which an upload workload never
        // reaches — a defaulted breaker would never open.
        CircuitBreakerStrategyOptions options = CloudStorageResilience.CreateCircuitBreakerOptions();

        options.MinimumThroughput.Should().Be(CloudStorageResilience.MinimumThroughput).And.BeLessThan(100);
        options.FailureRatio.Should().Be(CloudStorageResilience.FailureRatio);
    }

    [Fact]
    public void Pipeline_ShouldCapASingleCallAtTheHouseTimeout()
    {
        CloudStorageResilience.Timeout.Should().Be(TimeSpan.FromSeconds(10));

        // Retries must not push a failing call far past that budget.
        CloudStorageResilience.RetryDelay.Should().BeLessThan(CloudStorageResilience.Timeout);
    }

    [Fact]
    public async Task Pipeline_WhenACallOutlastsItsTimeout_ShouldAbandonIt()
    {
        ResiliencePipeline pipeline = new ResiliencePipelineBuilder().AddTimeout(TimeSpan.FromMilliseconds(20)).Build();

        Func<Task> act = async () =>
            await pipeline.ExecuteAsync(async ct => await Task.Delay(TimeSpan.FromSeconds(5), ct));

        await act.Should().ThrowAsync<TimeoutRejectedException>();
    }
}
