using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Identity.Api.Auth;

/// <summary>
/// Integration tests for admin authentication endpoints verifying login validation,
/// password management, OTP flows, and sign-out operations against a real PostgreSQL
/// database through the full API pipeline.
/// </summary>
[Collection("Database")]
public class AdminAuthEndpointTests(PostgresFixture db) : BaseApiTest(db)
{
    private const string AuthUrl = ApiRoutes.Admin.Auth;

    [Fact]
    public async Task Login_WithEmptyEmail_ReturnsValidationError()
    {
        Client.ClearAuthentication();
        var request = new { Email = "", Password = "ValidPass1" };

        var response = await Client.PostAsJsonAsync($"{AuthUrl}/login", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Login_WithEmptyPassword_ReturnsValidationError()
    {
        Client.ClearAuthentication();
        var request = new { Email = User.SuperAdminEmail, Password = "" };

        var response = await Client.PostAsJsonAsync($"{AuthUrl}/login", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Login_WithNonExistentEmail_ReturnsNotFoundOrBadRequest()
    {
        Client.ClearAuthentication();
        var request = new { Email = "nobody@nowhere.com", Password = "ValidPass1" };

        var response = await Client.PostAsJsonAsync($"{AuthUrl}/login", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ForgotPassword_WithEmptyEmail_ReturnsValidationError()
    {
        Client.ClearAuthentication();
        var request = new { Email = "" };

        var response = await Client.PostAsJsonAsync($"{AuthUrl}/forgot-password", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task ResetPassword_WithEmptyFields_ReturnsValidationError()
    {
        Client.ClearAuthentication();
        var request = new
        {
            Email = "",
            Code = "",
            NewPassword = "",
        };

        var response = await Client.PostAsJsonAsync($"{AuthUrl}/reset-password", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task ChangePassword_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new { OldPassword = "OldPass1", NewPassword = "NewPass1" };

        var response = await Client.PatchAsJsonAsync($"{AuthUrl}/change-password", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ChangePassword_WithWeakPassword_ReturnsValidationError()
    {
        Client.AuthenticateAsAdmin();
        var request = new { OldPassword = "OldPass1", NewPassword = "weak" };

        var response = await Client.PatchAsJsonAsync($"{AuthUrl}/change-password", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task SignOut_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new { RefreshToken = "some-token" };

        var response = await Client.PostAsJsonAsync($"{AuthUrl}/sign-out", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SignOutAll_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PostAsync($"{AuthUrl}/sign-out-all", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task VerifyOtp_WithEmptyFields_ReturnsValidationError()
    {
        Client.ClearAuthentication();
        var request = new
        {
            Email = "",
            Code = "",
            Purpose = "",
        };

        var response = await Client.PostAsJsonAsync($"{AuthUrl}/verify-otp", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task ResendOtp_WithEmptyEmail_ReturnsValidationError()
    {
        Client.ClearAuthentication();
        var request = new { Email = "", Purpose = "EmailVerification" };

        var response = await Client.PostAsJsonAsync($"{AuthUrl}/resend-otp", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }
}
