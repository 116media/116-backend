using System.Security.Cryptography;
using System.Text;
using _116.Identity.Application.Session.Constants;
using _116.Identity.Application.Session.UseCases.Public.Commands.RefreshToken.V1;
using _116.Identity.Application.Shared.Errors.Messages;
using _116.Identity.Application.Shared.Exceptions;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Mailer.Infrastructure.Persistence;
using _116.Tests.Fixtures.Builders.Requests.Identity;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Session.UseCases.Public.Commands.RefreshToken.V1;

/// <summary>
/// Integration tests for the PublicRefreshToken endpoint.
/// </summary>
[Collection("Database")]
public class PublicRefreshTokenEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string RefreshTokenUrl =
        $"{ApiRoutes.Public.Base}/{SessionRouteConstants.Endpoint}/{SessionRouteConstants.RefreshToken}";

    [Fact]
    public async Task RefreshToken_WithNoToken_ReturnsForbidden()
    {
        Client.ClearAuthentication();

        var response = await Client.PostAsync(RefreshTokenUrl, null);

        await response.ShouldBeProblem<RefreshTokenExpiryException>(
            HttpStatusCode.Forbidden,
            Localized<AuthenticationErrorMessage>(m => m.InvalidRefreshToken())
        );
    }

    [Fact]
    public async Task RefreshToken_WithInvalidTokenInBody_ReturnsForbidden()
    {
        Client.ClearAuthentication();

        // A random, unseeded refresh token is not valid for any session.
        var request = new PublicRefreshTokenRequestBuilder().Build();

        var response = await Client.PostAsJsonAsync(RefreshTokenUrl, request);

        await response.ShouldBeProblem<RefreshTokenExpiryException>(
            HttpStatusCode.Forbidden,
            Localized<AuthenticationErrorMessage>(m => m.InvalidRefreshToken())
        );
    }

    [Fact]
    public async Task RefreshToken_WithValidTokenInBody_ReturnsNewTokens()
    {
        const string rawRefreshToken = "test-valid-refresh-token-for-visitor";
        string refreshTokenHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(rawRefreshToken)));

        await SeedAsync<IdentityDbContext>(ctx =>
        {
            SessionEntity session = SessionFactory.CreateWithRefreshTokenHash(TestUser.VisitorId, refreshTokenHash);
            ctx.Sessions.Add(session);
        });

        Client.ClearAuthentication();
        var request = new PublicRefreshTokenRequestBuilder().WithRefreshToken(rawRefreshToken).Build();

        var response = await Client.PostAsJsonAsync(RefreshTokenUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicRefreshTokenMobileResponse body = await response.ReadAsAsync<PublicRefreshTokenMobileResponse>();
        body.User.Id.Should().Be(TestUser.VisitorId);
        body.AccessToken.Should().NotBeNullOrEmpty();
        body.RefreshToken.Should().NotBeNullOrEmpty();
        body.AccessTokenExpiresAt.Should().BeAfter(DateTime.UtcNow);
        body.RefreshTokenExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task RefreshToken_WithATokenFromARevokedSession_RevokesTheAccountAndAlertsTheOwner()
    {
        const string replayedToken = "test-replayed-refresh-token-for-visitor";
        string replayedHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(replayedToken)));

        var survivingSessionId = Guid.NewGuid();
        await SeedAsync<IdentityDbContext>(ctx =>
        {
            ctx.Sessions.Add(SessionFactory.CreateRevokedWithRefreshTokenHash(TestUser.VisitorId, replayedHash));
            ctx.Sessions.Add(SessionFactory.CreateWithId(survivingSessionId, TestUser.VisitorId));
        });

        Client.ClearAuthentication();
        var request = new PublicRefreshTokenRequestBuilder().WithRefreshToken(replayedToken).Build();

        var response = await Client.PostAsJsonAsync(RefreshTokenUrl, request);

        await response.ShouldBeProblem<RefreshTokenExpiryException>(
            HttpStatusCode.Forbidden,
            Localized<AuthenticationErrorMessage>(m => m.InvalidRefreshToken())
        );

        await using IdentityDbContext identityContext = CreateDbContext<IdentityDbContext>();
        SessionEntity surviving = await identityContext.Sessions.SingleAsync(s => s.Id == survivingSessionId);
        surviving.IsRevoked.Should().BeTrue();

        await using MailerDbContext mailerContext = CreateDbContext<MailerDbContext>();
        var outbox = await mailerContext
            .OutboxEmails.Where(o => o.RecipientAddress == TestUser.VisitorEmail)
            .ToListAsync();
        outbox.Should().ContainSingle(o => o.Template == "RefreshTokenReplayAlert");
    }
}
