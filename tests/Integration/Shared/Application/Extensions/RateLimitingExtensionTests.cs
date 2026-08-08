using System.Text.Json;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Shared.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace _116.Integration.Tests.Shared.Application.Extensions;

/// <summary>
/// Verifies that <c>RateLimitingExtension.AddRateLimiting</c> is wired into the real HTTP
/// pipeline and that every named policy it registers actually rejects traffic at the limit its
/// constants declare.
/// </summary>
/// <remarks>
/// These tests run against <see cref="RateLimitedApiFixture" />, the only host that keeps the
/// production policies in place. Every policy is registered as a single host-wide limiter whose
/// permits are never restored between tests, so the theory below must remain the sole consumer of
/// every policy on this host: a second test driving a policy a row already exhausted would steal
/// its permits and both would be flaky. The permitted requests are deliberately sent
/// unauthenticated and with no body: the rate limiter runs before authentication and before model
/// binding, so a permit is consumed regardless of the outcome, which keeps these tests focused on
/// the limiter rather than on endpoint behaviour.
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
    /// Supplies every named rate limit policy paired with the permit limit its constants declare
    /// and a request against an endpoint that carries that policy. Sourcing the limit from the
    /// production constant is what makes a row prove the configured number rather than a copy of
    /// it. Each route is named beside its endpoint file so the pairing can be re-verified.
    /// </summary>
    /// <returns>Policy name, permit limit, HTTP method, and route for each registered policy.</returns>
    public static TheoryData<string, int, string, string> Policies() =>
        new()
        {
            {
                RateLimitPolicies.Authentication,
                AuthenticationRateLimitConstants.PermitLimit,
                HttpMethod.Post.Method,
                Routes.Public.Auth.Login()
            },
            {
                RateLimitPolicies.Otp,
                OtpRateLimitConstants.PermitLimit,
                HttpMethod.Post.Method,
                Routes.Public.Auth.VerifyOtp()
            },
            {
                RateLimitPolicies.PasswordManagement,
                PasswordManagementRateLimitConstants.PermitLimit,
                HttpMethod.Post.Method,
                Routes.Public.Auth.ForgotPassword()
            },
            {
                RateLimitPolicies.FileUpload,
                FileUploadRateLimitConstants.TokenLimit,
                HttpMethod.Patch.Method,
                Routes.Public.Me.Avatar()
            },
            {
                RateLimitPolicies.DataExport,
                DataExportRateLimitConstants.TokenLimit,
                HttpMethod.Get.Method,
                Routes.Admin.Sessions.Export()
            },
            {
                RateLimitPolicies.ContentBrowsing,
                ContentBrowsingRateLimitConstants.PermitLimit,
                HttpMethod.Get.Method,
                ApiRoutes.Public.Articles
            },
            {
                RateLimitPolicies.UserProfile,
                UserProfileRateLimitConstants.PermitLimit,
                HttpMethod.Get.Method,
                Routes.Public.Me.Profile()
            },
            {
                RateLimitPolicies.SessionManagement,
                SessionManagementRateLimitConstants.PermitLimit,
                HttpMethod.Post.Method,
                Routes.Public.Auth.SignOut()
            },
            {
                RateLimitPolicies.AdminMetrics,
                AdminMetricsRateLimitConstants.PermitLimit,
                HttpMethod.Get.Method,
                Routes.Admin.Sessions.Metrics()
            },
            {
                RateLimitPolicies.ContentContribution,
                ContentContributionRateLimitConstants.PermitLimit,
                HttpMethod.Post.Method,
                Routes.Public.LyricsSubmissionsAndRevisions.RevisionVotes(Guid.Empty)
            },
        };

    [Theory]
    [MemberData(nameof(Policies))]
    public async Task EveryNamedPolicy_RejectsWithTooManyRequests_AtItsConfiguredLimit(
        string policy,
        int permitLimit,
        string method,
        string route
    )
    {
        var httpMethod = new HttpMethod(method);

        using HttpResponseMessage rejected = await ExhaustAsync(
            permitLimit,
            () => _client.SendAsync(new HttpRequestMessage(httpMethod, route))
        );

        rejected
            .StatusCode.Should()
            .Be(
                HttpStatusCode.TooManyRequests,
                "the {0} policy must be registered and reject the request after {1} permits",
                policy,
                permitLimit
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
