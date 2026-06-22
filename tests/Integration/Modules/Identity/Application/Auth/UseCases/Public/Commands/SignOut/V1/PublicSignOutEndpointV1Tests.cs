using System.Security.Cryptography;
using System.Text;
using _116.Identity.Application.Auth.UseCases.Public.Commands.SignOut.V1;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Builders.Requests.Identity;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.SignOut.V1;

/// <summary>
/// Integration tests for the PublicSignOut endpoint.
/// </summary>
[Collection("Database")]
public class PublicSignOutEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task SignOut_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new PublicSignOutRequestBuilder().WithRefreshToken(string.Empty).Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Auth.SignOut(), request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SignOut_AsVisitor_WithEmptyRefreshToken_ReturnsValidationError()
    {
        Client.AuthenticateAsVisitor();
        var request = new PublicSignOutRequestBuilder().WithRefreshToken(string.Empty).Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Auth.SignOut(), request);

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Verifies that a Visitor with a valid refresh token can sign out, returning a success
    /// payload and revoking the matching session in the database.
    /// </summary>
    [Fact]
    public async Task SignOut_AsVisitor_WithValidRefreshToken_RevokesSession()
    {
        var rawRefreshToken = $"visitor-signout-{Guid.NewGuid():N}";
        var refreshTokenHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(rawRefreshToken)));

        var session = await SeedAsync<IdentityDbContext, SessionEntity>(ctx =>
        {
            var entity = SessionFactory.CreateWithRefreshTokenHash(TestUser.VisitorId, refreshTokenHash);
            ctx.Sessions.Add(entity);
            return entity;
        });

        Client.AuthenticateAsVisitor();

        var request = new PublicSignOutRequestBuilder().WithRefreshToken(rawRefreshToken).Build();
        var response = await Client.PostAsJsonAsync(Routes.Public.Auth.SignOut(), request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicSignOutResponse body = await response.ReadAsAsync<PublicSignOutResponse>();
        body.IsSuccess.Should().BeTrue();

        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        var revoked = await verifyContext.Sessions.FirstAsync(s => s.Id == session.Id);
        revoked.IsRevoked.Should().BeTrue();
    }

    [Fact]
    public async Task SignOutAll_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PostAsync(Routes.Public.Auth.SignOutAll(), null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
