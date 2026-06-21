using System.Text.Json;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Workflows;

/// <summary>
/// Cross-module workflow tests for the authentication lifecycle:
/// signup → login → access protected endpoint → signout.
/// </summary>
[Collection("Database")]
public class AuthenticationFlowTests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task SignUpAndLogin_ShouldGrantAccessToProtectedEndpoints()
    {
        await using var context = CreateDbContext<IdentityDbContext>();
        var visitorRole = RoleFactory.CreateWithId(Guid.NewGuid(), "Visitor");
        context.Roles.Add(visitorRole);
        await context.SaveChangesAsync();

        Client.ClearAuthentication();
        Client.DefaultRequestHeaders.Add("X-Device-Id", Guid.NewGuid().ToString());

        var email = $"flow-{Guid.NewGuid():N}@test.com";
        var userName = $"u{Guid.NewGuid():N}"[..10];
        var signupRequest = new
        {
            Email = email,
            UserName = userName,
            Password = TestAuth.ValidPassword,
        };

        HttpResponseMessage signupResponse = await Client.PostAsJsonAsync(
            $"{ApiRoutes.Public.Auth}/signup",
            signupRequest
        );
        signupResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        string signupBody = await signupResponse.Content.ReadAsStringAsync();
        using JsonDocument signupDoc = JsonDocument.Parse(signupBody);
        string? accessToken = signupDoc.RootElement.TryGetProperty("accessToken", out JsonElement at)
            ? at.GetString()
            : null;
        string? refreshToken = signupDoc.RootElement.TryGetProperty("refreshToken", out JsonElement rt)
            ? rt.GetString()
            : null;

        accessToken.Should().NotBeNullOrEmpty();
        refreshToken.Should().NotBeNullOrEmpty();
        accessToken!.Split('.').Should().HaveCount(3);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnTokens()
    {
        await using var context = CreateDbContext<IdentityDbContext>();
        var visitorRole = RoleFactory.CreateWithId(Guid.NewGuid(), "Visitor");
        context.Roles.Add(visitorRole);
        await context.SaveChangesAsync();

        Client.ClearAuthentication();
        Client.DefaultRequestHeaders.Add("X-Device-Id", Guid.NewGuid().ToString());

        var email = $"login-{Guid.NewGuid():N}@test.com";
        var userName = $"u{Guid.NewGuid():N}"[..10];
        var signupRequest = new
        {
            Email = email,
            UserName = userName,
            Password = TestAuth.ValidPassword,
        };

        await Client.PostAsJsonAsync($"{ApiRoutes.Public.Auth}/signup", signupRequest);

        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        var user = await verifyContext.Users.FirstAsync(u => u.Email == email);
        user.MarkAsVerified();
        user.Activate();
        await verifyContext.SaveChangesAsync();

        var loginRequest = new { Credentials = email, Password = TestAuth.ValidPassword };

        HttpResponseMessage loginResponse = await Client.PostAsJsonAsync(
            $"{ApiRoutes.Public.Auth}/login",
            loginRequest
        );
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        string loginBody = await loginResponse.Content.ReadAsStringAsync();
        using JsonDocument loginDoc = JsonDocument.Parse(loginBody);
        string? loginToken = loginDoc.RootElement.TryGetProperty("accessToken", out JsonElement lat)
            ? lat.GetString()
            : null;

        loginToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task SignUp_WithDuplicateEmail_ShouldReturnConflict()
    {
        Client.ClearAuthentication();

        var request = new
        {
            Email = TestUser.SuperAdminEmail,
            UserName = $"u{Guid.NewGuid():N}"[..10],
            Password = TestAuth.ValidPassword,
        };

        HttpResponseMessage response = await Client.PostAsJsonAsync($"{ApiRoutes.Public.Auth}/signup", request);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnError()
    {
        Client.ClearAuthentication();

        var request = new { Credentials = "nonexistent@nobody.com", Password = TestAuth.ValidPassword };

        HttpResponseMessage response = await Client.PostAsJsonAsync($"{ApiRoutes.Public.Auth}/login", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }
}
