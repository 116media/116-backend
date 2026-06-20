using System.Text.Json;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Identity.Api.Auth;

/// <summary>
/// Integration tests for the public authentication endpoints verifying login,
/// signup, password management, OTP, social login, and sign-out operations
/// against a real PostgreSQL database through the full API pipeline.
/// </summary>
[Collection("Database")]
public class PublicAuthEndpointTests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task Login_WithEmptyCredentials_ReturnsValidationError()
    {
        Client.ClearAuthentication();
        var request = new { Credentials = "", Password = "" };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Public.Auth}/login", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Login_WithNonExistentCredentials_ReturnsError()
    {
        Client.ClearAuthentication();
        var request = new { Credentials = "nobody@nowhere.com", Password = "Test123!abc" };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Public.Auth}/login", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SignUp_WithValidData_ReturnsCreated()
    {
        await using var context = CreateDbContext<IdentityDbContext>();
        var visitorRole = RoleFactory.CreateWithId(Guid.NewGuid(), "Visitor");
        context.Roles.Add(visitorRole);
        await context.SaveChangesAsync();

        Client.ClearAuthentication();
        Client.DefaultRequestHeaders.Add("X-Device-Id", Guid.NewGuid().ToString());
        var email = $"s{Guid.NewGuid():N}@test.com";
        var userName = $"u{Guid.NewGuid():N}"[..10];
        var request = new
        {
            Email = email,
            UserName = userName,
            Password = "Test123!abc",
        };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Public.Auth}/signup", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task SignUp_WithDuplicateEmail_ReturnsConflict()
    {
        Client.ClearAuthentication();
        var request = new
        {
            Email = User.SuperAdminEmail,
            UserName = $"u{Guid.NewGuid():N}"[..10],
            Password = "Test123!abc",
        };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Public.Auth}/signup", request);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task SignUp_WithEmptyEmail_ReturnsValidationError()
    {
        Client.ClearAuthentication();
        var request = new
        {
            Email = "",
            UserName = "validuser",
            Password = "Test123!abc",
        };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Public.Auth}/signup", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task SignUp_WithWeakPassword_ReturnsValidationError()
    {
        Client.ClearAuthentication();
        var request = new
        {
            Email = $"s{Guid.NewGuid():N}@test.com",
            UserName = $"u{Guid.NewGuid():N}"[..10],
            Password = "abc",
        };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Public.Auth}/signup", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task SignUp_WithShortUsername_ReturnsValidationError()
    {
        Client.ClearAuthentication();
        var request = new
        {
            Email = $"s{Guid.NewGuid():N}@test.com",
            UserName = "ab",
            Password = "Test123!abc",
        };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Public.Auth}/signup", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task ForgotPassword_WithInvalidEmail_ReturnsValidationError()
    {
        Client.ClearAuthentication();
        var request = new { Email = "not-an-email" };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Public.Auth}/forgot-password", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task ChangePassword_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new { OldPassword = "Old123!abc", NewPassword = "New123!abc" };

        var response = await Client.PatchAsJsonAsync($"{ApiRoutes.Public.Auth}/change-password", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ChangePassword_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();
        var request = new { OldPassword = "Old123!abc", NewPassword = "New123!abc" };

        var response = await Client.PatchAsJsonAsync($"{ApiRoutes.Public.Auth}/change-password", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task SetPassword_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new { Password = "Test123!abc" };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Public.Auth}/set-password", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SignOut_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new { RefreshToken = "" };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Public.Auth}/sign-out", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SignOut_AsVisitor_WithEmptyRefreshToken_ReturnsValidationError()
    {
        Client.AuthenticateAsVisitor();
        var request = new { RefreshToken = "" };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Public.Auth}/sign-out", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task SignOutAll_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PostAsync($"{ApiRoutes.Public.Auth}/sign-out-all", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SocialLogin_WithInvalidProvider_ReturnsValidationError()
    {
        Client.ClearAuthentication();
        var request = new
        {
            Email = "social@test.com",
            UserName = "socialuser",
            AvatarUrl = "https://example.com/avatar.png",
            Provider = "InvalidProvider",
        };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Public.Auth}/social-login", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }
}
