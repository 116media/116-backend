using _116.BuildingBlocks.Constants.RateLimit;
using _116.Shared.Application.Builders.RateLimit;
using _116.Shared.Application.Exceptions;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Shared.Application.Builders.RateLimit;

/// <summary>
/// Unit tests for <see cref="AccountRateLimiter"/>. Consuming more than a policy's window for one
/// account throws; a different account is unaffected; unknown policies and blank keys are no-ops; and
/// the key is normalized so casing and whitespace share one bucket.
/// </summary>
public class AccountRateLimiterTests
{
    private static async Task ConsumeAsync(AccountRateLimiter limiter, string policy, string key, int times)
    {
        for (var i = 0; i < times; i++)
        {
            await limiter.EnsureWithinLimitAsync(policy, key, CancellationToken.None);
        }
    }

    [Fact]
    public async Task EnsureWithinLimit_ExhaustsAfterThePermitLimit()
    {
        // Arrange
        await using var limiter = new AccountRateLimiter();
        string key = $"a{Guid.NewGuid():N}@x.com";
        await ConsumeAsync(
            limiter,
            RateLimitPolicies.Authentication,
            key,
            AuthenticationRateLimitConstants.PermitLimit
        );

        // Act
        Func<Task> act = () =>
            limiter.EnsureWithinLimitAsync(RateLimitPolicies.Authentication, key, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<RateLimitExceededException>();
    }

    [Fact]
    public async Task EnsureWithinLimit_ForADifferentAccount_IsUnaffected()
    {
        // Arrange — exhaust account A
        await using var limiter = new AccountRateLimiter();
        await ConsumeAsync(
            limiter,
            RateLimitPolicies.Authentication,
            $"a{Guid.NewGuid():N}@x.com",
            AuthenticationRateLimitConstants.PermitLimit
        );

        // Act — account B has its own bucket
        Func<Task> act = () =>
            limiter.EnsureWithinLimitAsync(
                RateLimitPolicies.Authentication,
                $"b{Guid.NewGuid():N}@x.com",
                CancellationToken.None
            );

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task EnsureWithinLimit_NormalizesKey_SoCasingAndWhitespaceShareBucket()
    {
        // Arrange — exhaust the window using the plain lowercase form
        await using var limiter = new AccountRateLimiter();
        await ConsumeAsync(
            limiter,
            RateLimitPolicies.Authentication,
            "shared@x.com",
            AuthenticationRateLimitConstants.PermitLimit
        );

        // Act — a padded, upper-cased variant must map to the same bucket
        Func<Task> act = () =>
            limiter.EnsureWithinLimitAsync(
                RateLimitPolicies.Authentication,
                "  SHARED@X.COM  ",
                CancellationToken.None
            );

        // Assert
        await act.Should().ThrowAsync<RateLimitExceededException>();
    }

    [Fact]
    public async Task EnsureWithinLimit_ForAnUnknownPolicy_IsNoop()
    {
        // Arrange
        await using var limiter = new AccountRateLimiter();

        // Act
        Func<Task> act = () => ConsumeAsync(limiter, "NonexistentPolicy", "x@x.com", times: 50);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task EnsureWithinLimit_WithBlankKey_IsNoop()
    {
        // Arrange
        await using var limiter = new AccountRateLimiter();

        // Act
        Func<Task> act = () => ConsumeAsync(limiter, RateLimitPolicies.Authentication, "   ", times: 50);

        // Assert
        await act.Should().NotThrowAsync();
    }
}
