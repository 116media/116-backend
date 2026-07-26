using System.Text.Json;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Shared.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace _116.Integration.Tests.Shared.Application.Extensions;

/// <summary>
/// Verifies that <c>RateLimitingExtension.AddRateLimiting</c> is wired into the real HTTP
/// pipeline and that each of its three algorithms actually rejects traffic.
/// </summary>
/// <remarks>
/// These tests run against <see cref="RateLimitedApiFixture" />, the only host that keeps the
/// production policies in place. Every policy is registered as a single host-wide limiter, so
/// each test drives a different policy and no test can steal another's permits. The permitted
/// requests are deliberately sent unauthenticated or with an invalid body: the rate limiter runs
/// before authentication and before model binding, so a permit is consumed regardless of the
/// outcome, which keeps these tests focused on the limiter rather than on endpoint behaviour.
/// </remarks>
/// <param name="db">The dedicated Testcontainer database and rate-limited application host.</param>
[Collection("RateLimiting")]
public class RateLimitingExtensionTests(RateLimitedPostgresFixture db) : IDisposable
{
    private readonly HttpClient _client = db.Api.CreateClient();

    /// <inheritdoc />
    public void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Drives the sliding window tier by exhausting the <c>Otp</c> policy on the public
    /// verify-OTP endpoint, proving that the sliding window policies configured by
    /// <c>ConfigureSlidingWindowPolicies</c> are registered and enforced.
    /// </summary>
    [Fact]
    public async Task SlidingWindowPolicy_RejectsWithTooManyRequests_WhenOtpPermitLimitExceeded()
    {
        object body = new
        {
            email = "rate-limit-probe@116.test",
            code = "000000",
            purpose = "AccountVerification",
        };

        using HttpResponseMessage rejected = await ExhaustAsync(
            OtpRateLimitConstants.PermitLimit,
            () => _client.PostAsJsonAsync(Routes.Public.Auth.VerifyOtp(), body)
        );

        await ShouldBeRateLimitRejectionAsync(rejected);
    }

    /// <summary>
    /// Drives the token bucket tier by exhausting the <c>DataExport</c> policy on the admin
    /// session export endpoint, proving that the token bucket policies configured by
    /// <c>ConfigureTokenBucketPolicies</c> are registered and enforced. The replenishment period
    /// is a full minute, so no token is returned to the bucket during the test.
    /// </summary>
    [Fact]
    public async Task TokenBucketPolicy_RejectsWithTooManyRequests_WhenDataExportTokensExhausted()
    {
        using HttpResponseMessage rejected = await ExhaustAsync(
            DataExportRateLimitConstants.TokenLimit,
            () => _client.GetAsync(Routes.Admin.Sessions.Export())
        );

        await ShouldBeRateLimitRejectionAsync(rejected);
    }

    /// <summary>
    /// Drives the fixed window tier by exhausting the <c>ContentContribution</c> policy on the
    /// lyrics revision voting endpoint, proving that the fixed window policies configured by
    /// <c>ConfigureFixedWindowPolicies</c> are registered and enforced.
    /// </summary>
    [Fact]
    public async Task FixedWindowPolicy_RejectsWithTooManyRequests_WhenContentContributionPermitLimitExceeded()
    {
        string route = Routes.Public.LyricsSubmissionsAndRevisions.RevisionVotes(Guid.NewGuid());
        object body = new { vote = "Approve", comment = (string?)null };

        using HttpResponseMessage rejected = await ExhaustAsync(
            ContentContributionRateLimitConstants.PermitLimit,
            () => _client.PostAsJsonAsync(route, body)
        );

        await ShouldBeRateLimitRejectionAsync(rejected);
    }

    /// <summary>
    /// Sends exactly <paramref name="permitLimit" /> requests, asserting that every one of them
    /// is admitted by the limiter, then sends one more and returns it for rejection assertions.
    /// </summary>
    /// <param name="permitLimit">The number of requests the policy is configured to admit.</param>
    /// <param name="send">Issues a single request against the policy under test.</param>
    /// <returns>The response to the request that exceeded the limit.</returns>
    private static async Task<HttpResponseMessage> ExhaustAsync(int permitLimit, Func<Task<HttpResponseMessage>> send)
    {
        for (var attempt = 1; attempt <= permitLimit; attempt++)
        {
            using HttpResponseMessage permitted = await send();

            permitted
                .StatusCode.Should()
                .NotBe(
                    HttpStatusCode.TooManyRequests,
                    "request {0} of {1} is still within the configured permit limit",
                    attempt,
                    permitLimit
                );
        }

        return await send();
    }

    /// <summary>
    /// Asserts that the response is the rejection produced by the <c>OnRejected</c> callback:
    /// the <see cref="RateLimitExceededException" /> it throws is translated by the global
    /// exception pipeline into a 429 ProblemDetails carrying a <c>Retry-After</c> header.
    /// </summary>
    /// <param name="response">The response to the request that exceeded the limit.</param>
    private static async Task ShouldBeRateLimitRejectionAsync(HttpResponseMessage response)
    {
        response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);

        response
            .Headers.RetryAfter.Should()
            .NotBeNull("the rejection handler writes the lease retry-after metadata to the response");
        response.Headers.RetryAfter!.Delta.Should().NotBeNull();
        response.Headers.RetryAfter.Delta!.Value.Should().BeGreaterThanOrEqualTo(TimeSpan.Zero);

        string raw = await response.Content.ReadAsStringAsync();
        raw.Should().NotBeNullOrWhiteSpace();

        ProblemDetails? problem = JsonSerializer.Deserialize<ProblemDetails>(
            raw,
            new JsonSerializerOptions(JsonSerializerDefaults.Web)
        );

        problem.Should().NotBeNull();
        problem!.Status.Should().Be((int)HttpStatusCode.TooManyRequests);
        problem.Title.Should().Be(nameof(RateLimitExceededException));
        problem.Detail.Should().NotBeNullOrWhiteSpace();
        problem.Extensions.Should().ContainKey("traceId");
    }
}
