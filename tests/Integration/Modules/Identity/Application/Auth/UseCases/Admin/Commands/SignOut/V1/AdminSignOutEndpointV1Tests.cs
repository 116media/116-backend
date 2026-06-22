using System.Security.Cryptography;
using System.Text;
using _116.Identity.Application.Auth.Constants;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.SignOut.V1;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Builders.Requests.Identity;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Auth.UseCases.Admin.Commands.SignOut.V1;

/// <summary>
/// Integration tests for the AdminSignOut endpoint.
/// </summary>
[Collection("Database")]
public class AdminSignOutEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string AuthUrl = ApiRoutes.Admin.Auth;
    private const string SignOutUrl = $"{AuthUrl}/{AuthRouteConstants.SignOut}";
    private const string SignOutAllUrl = $"{AuthUrl}/{AuthRouteConstants.SignOutAll}";

    [Fact]
    public async Task SignOut_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new AdminSignOutRequestBuilder().WithRefreshToken("some-token").Build();

        var response = await Client.PostAsJsonAsync(SignOutUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SignOutAll_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PostAsync(SignOutAllUrl, null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// Verifies that a SuperAdmin user with a valid refresh token can successfully sign out,
    /// revoking the session associated with the provided refresh token.
    /// </summary>
    [Fact]
    public async Task SignOut_AsSuperAdmin_WithValidRefreshToken_ReturnsOk()
    {
        var rawRefreshToken = "test-refresh-token-for-signout";
        var refreshTokenHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(rawRefreshToken)));

        var session = await SeedAsync<IdentityDbContext, SessionEntity>(ctx =>
        {
            var entity = SessionFactory.CreateWithRefreshTokenHash(TestUser.SuperAdminId, refreshTokenHash);
            ctx.Sessions.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();

        var request = new AdminSignOutRequestBuilder().WithRefreshToken(rawRefreshToken).Build();
        var response = await Client.PostAsJsonAsync(SignOutUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminSignOutResponse body = await response.ReadAsAsync<AdminSignOutResponse>();
        body.IsSuccess.Should().BeTrue();

        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        var revoked = await verifyContext.Sessions.FirstAsync(s => s.Id == session.Id);
        revoked.IsRevoked.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that signing out with an inactive account returns a 423 Locked response,
    /// because the account-status authorization handler rejects the request before it
    /// reaches the handler.
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

        var request = new AdminSignOutRequestBuilder().WithRefreshToken("some-refresh-token").Build();
        var response = await Client.PostAsJsonAsync(SignOutUrl, request);

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

        var request = new AdminSignOutRequestBuilder()
            .WithRefreshToken("this-token-does-not-match-any-session")
            .Build();
        var response = await Client.PostAsJsonAsync(SignOutUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminSignOutResponse body = await response.ReadAsAsync<AdminSignOutResponse>();
        body.IsSuccess.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that sending a sign-out request with an empty refresh token
    /// returns a 400 Bad Request due to validation failure.
    /// </summary>
    [Fact]
    public async Task SignOut_WithEmptyRefreshToken_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();

        var request = new AdminSignOutRequestBuilder().WithRefreshToken(string.Empty).Build();
        var response = await Client.PostAsJsonAsync(SignOutUrl, request);

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }
}
