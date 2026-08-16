using _116.BuildingBlocks.Constants.RateLimit;

namespace _116.Integration.Tests.Shared.Application.RateLimiting;

/// <summary>
/// Verifies the per-account throttle applied inside the pre-auth handlers. This host disables the
/// middleware limiter, so a rejection here can only come from the per-account throttle — proving one
/// account is limited independently of caller IP, and that a different account keeps its own window.
/// </summary>
/// <remarks>
/// Forgot-password is the driver because it is anonymous and enumeration-safe: it throttles for every
/// email before touching the database, so no seeding is needed and unknown accounts are throttled too.
/// Each test uses a unique email so the host-wide throttle cannot leak permits between assertions.
/// </remarks>
/// <param name="db">The dedicated database and account-throttled application host.</param>
[Collection("AccountRateLimiting")]
public class AccountRateLimitThrottleTests(AccountRateLimitedPostgresFixture db) : IDisposable
{
    private readonly HttpClient _client = db.Api.CreateClient();

    /// <inheritdoc />
    public void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }

    private Task<HttpResponseMessage> ForgotPasswordAsync(string email) =>
        _client.PostAsJsonAsync(Routes.Public.Auth.ForgotPassword(), new { email });

    [Fact]
    public async Task ForgotPassword_ThrottlesPerAccount_AfterThePasswordManagementLimit()
    {
        string email = $"throttle-{Guid.NewGuid():N}@test.com";

        for (var attempt = 1; attempt <= PasswordManagementRateLimitConstants.PermitLimit; attempt++)
        {
            using HttpResponseMessage permitted = await ForgotPasswordAsync(email);
            permitted
                .StatusCode.Should()
                .NotBe(HttpStatusCode.TooManyRequests, "attempt {0} is within the account limit", attempt);
        }

        using HttpResponseMessage rejected = await ForgotPasswordAsync(email);
        rejected.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);

        // A different account has its own window — proving the throttle is per-account, not per-host.
        using HttpResponseMessage otherAccount = await ForgotPasswordAsync($"other-{Guid.NewGuid():N}@test.com");
        otherAccount.StatusCode.Should().NotBe(HttpStatusCode.TooManyRequests);
    }
}
