using System.Security.Cryptography;
using System.Text;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Session.UseCases.Admin.Commands.RefreshToken.V1;

/// <summary>
/// Integration tests for the AdminRefreshToken endpoint.
/// </summary>
[Collection("Database")]
public class AdminRefreshTokenEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task RefreshToken_WithNoToken_ReturnsForbidden()
    {
        Client.ClearAuthentication();

        var response = await Client.PostAsync($"{ApiRoutes.Admin.Sessions}/refresh-token", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RefreshToken_WithInvalidTokenInBody_ReturnsForbidden()
    {
        Client.ClearAuthentication();
        var request = new { RefreshToken = "invalid-refresh-token" };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Admin.Sessions}/refresh-token", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>
    /// Verifies that refreshing with a valid token cookie returns 200 OK and new tokens.
    /// </summary>
    [Fact]
    public async Task RefreshToken_WithValidToken_ReturnsOk()
    {
        const string rawRefreshToken = "test-valid-refresh-token-for-admin";
        string refreshTokenHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(rawRefreshToken)));

        await using var context = CreateDbContext<IdentityDbContext>();
        var session = SessionFactory.CreateWithRefreshTokenHash(TestUser.SuperAdminId, refreshTokenHash);
        context.Sessions.Add(session);
        await context.SaveChangesAsync();

        Client.ClearAuthentication();

        var msg = new HttpRequestMessage(HttpMethod.Post, $"{ApiRoutes.Admin.Sessions}/refresh-token");
        msg.Headers.Add("Client-App", "Dashboard");
        msg.Headers.Add("Cookie", $"refreshToken={rawRefreshToken}");

        var response = await Client.SendAsync(msg);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies that refreshing with an invalid token cookie returns 403 Forbidden.
    /// </summary>
    [Fact]
    public async Task RefreshToken_WithInvalidToken_ReturnsForbidden()
    {
        Client.ClearAuthentication();

        var msg = new HttpRequestMessage(HttpMethod.Post, $"{ApiRoutes.Admin.Sessions}/refresh-token");
        msg.Headers.Add("Client-App", "Dashboard");
        msg.Headers.Add("Cookie", "refreshToken=this-token-does-not-exist-in-db");

        var response = await Client.SendAsync(msg);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>
    /// Verifies that refreshing with an empty token returns 403 Forbidden.
    /// </summary>
    [Fact]
    public async Task RefreshToken_WithEmptyToken_ReturnsForbidden()
    {
        Client.ClearAuthentication();

        var msg = new HttpRequestMessage(HttpMethod.Post, $"{ApiRoutes.Admin.Sessions}/refresh-token");
        msg.Headers.Add("Client-App", "Dashboard");
        msg.Headers.Add("Cookie", "refreshToken=");

        var response = await Client.SendAsync(msg);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
