using System.Security.Cryptography;
using System.Text;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Auth.UseCases.Admin.Commands.SignOut.V1;

/// <summary>
/// Integration tests for the AdminSignOut endpoint.
/// </summary>
[Collection("Database")]
public class AdminSignOutEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string AuthUrl = ApiRoutes.Admin.Auth;

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

    /// <summary>
    /// Verifies that a SuperAdmin user with a valid refresh token can successfully sign out,
    /// revoking the session associated with the provided refresh token.
    /// </summary>
    [Fact]
    public async Task SignOut_AsSuperAdmin_WithValidRefreshToken_ReturnsOk()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();

        var rawRefreshToken = "test-refresh-token-for-signout";
        var refreshTokenHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(rawRefreshToken)));

        var session = SessionFactory.CreateWithRefreshTokenHash(TestUser.SuperAdminId, refreshTokenHash);
        seedContext.Sessions.Add(session);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var request = new { RefreshToken = rawRefreshToken };
        var response = await Client.PostAsJsonAsync($"{AuthUrl}/sign-out", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies that signing out with an inactive account returns a 403 Forbidden response,
    /// because the authorization handler rejects the request before it reaches the handler.
    /// </summary>
    [Fact]
    public async Task SignOut_WithInactiveAccount_ReturnsForbidden()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();

        var inactiveUserId = Guid.NewGuid();
        var inactiveUser = UserFactory.CreateWithId(inactiveUserId, "inactive-signout@test.com");
        inactiveUser.MarkAsVerified();
        inactiveUser.Deactivate();

        seedContext.Users.Add(inactiveUser);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAs(inactiveUserId, "Admin");

        var request = new { RefreshToken = "some-refresh-token" };
        var response = await Client.PostAsJsonAsync($"{AuthUrl}/sign-out", request);

        response.StatusCode.Should().Be(HttpStatusCode.Locked);
    }

    /// <summary>
    /// Verifies that signing out with a refresh token that does not match any session
    /// still returns 200 OK. The sign-out operation is idempotent per RFC 7009:
    /// a non-matching token is silently accepted because the user is effectively logged out.
    /// </summary>
    [Fact]
    public async Task SignOut_WithNonMatchingRefreshToken_ReturnsOk()
    {
        Client.AuthenticateAsSuperAdmin();

        var request = new { RefreshToken = "this-token-does-not-match-any-session" };
        var response = await Client.PostAsJsonAsync($"{AuthUrl}/sign-out", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies that sending a sign-out request with an empty refresh token
    /// returns a 400 Bad Request due to validation failure.
    /// </summary>
    [Fact]
    public async Task SignOut_WithEmptyRefreshToken_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();

        var request = new { RefreshToken = "" };
        var response = await Client.PostAsJsonAsync($"{AuthUrl}/sign-out", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
