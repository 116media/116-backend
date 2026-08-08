using System.Security.Cryptography;
using System.Text;
using _116.Identity.Application.Session.UseCases.Admin.Commands.RefreshToken.V1;
using _116.Identity.Application.Shared.Errors.Messages;
using _116.Identity.Application.Shared.Exceptions;
using _116.Identity.Domain.Entities;
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

        var response = await Client.PostAsync(Routes.Admin.Sessions.RefreshToken(), null);

        await response.ShouldBeProblem<RefreshTokenExpiryException>(
            HttpStatusCode.Forbidden,
            Localized<AuthenticationErrorMessage>(m => m.InvalidRefreshToken())
        );
    }

    [Fact]
    public async Task RefreshToken_WithInvalidTokenInBody_ReturnsForbidden()
    {
        Client.ClearAuthentication();
        var request = new { RefreshToken = "invalid-refresh-token" };

        var response = await Client.PostAsJsonAsync(Routes.Admin.Sessions.RefreshToken(), request);

        await response.ShouldBeProblem<RefreshTokenExpiryException>(
            HttpStatusCode.Forbidden,
            Localized<AuthenticationErrorMessage>(m => m.InvalidRefreshToken())
        );
    }

    [Fact]
    public async Task RefreshToken_WithValidToken_ReturnsOk()
    {
        const string rawRefreshToken = "test-valid-refresh-token-for-admin";
        string refreshTokenHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(rawRefreshToken)));

        await SeedAsync<IdentityDbContext>(ctx =>
        {
            SessionEntity session = SessionFactory.CreateWithRefreshTokenHash(TestUser.SuperAdminId, refreshTokenHash);
            ctx.Sessions.Add(session);
        });

        Client.ClearAuthentication();

        var msg = new HttpRequestMessage(HttpMethod.Post, Routes.Admin.Sessions.RefreshToken());
        msg.Headers.Add("Client-App", "Dashboard");
        msg.Headers.Add("Cookie", $"refreshToken={rawRefreshToken}");

        var response = await Client.SendAsync(msg);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminRefreshTokenResponse body = await response.ReadAsAsync<AdminRefreshTokenResponse>();
        body.User.Should().NotBeNull();
        body.User.Id.Should().Be(TestUser.SuperAdminId);
    }

    [Fact]
    public async Task RefreshToken_WithInvalidToken_ReturnsForbidden()
    {
        Client.ClearAuthentication();

        var msg = new HttpRequestMessage(HttpMethod.Post, Routes.Admin.Sessions.RefreshToken());
        msg.Headers.Add("Client-App", "Dashboard");
        msg.Headers.Add("Cookie", "refreshToken=this-token-does-not-exist-in-db");

        var response = await Client.SendAsync(msg);

        await response.ShouldBeProblem<RefreshTokenExpiryException>(
            HttpStatusCode.Forbidden,
            Localized<AuthenticationErrorMessage>(m => m.InvalidRefreshToken())
        );
    }

    [Fact]
    public async Task RefreshToken_WithEmptyToken_ReturnsForbidden()
    {
        Client.ClearAuthentication();

        var msg = new HttpRequestMessage(HttpMethod.Post, Routes.Admin.Sessions.RefreshToken());
        msg.Headers.Add("Client-App", "Dashboard");
        msg.Headers.Add("Cookie", "refreshToken=");

        var response = await Client.SendAsync(msg);

        await response.ShouldBeProblem<RefreshTokenExpiryException>(
            HttpStatusCode.Forbidden,
            Localized<AuthenticationErrorMessage>(m => m.InvalidRefreshToken())
        );
    }
}
