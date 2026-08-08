using _116.Identity.Application.Session.Constants;
using _116.Identity.Application.Session.UseCases.Public.Commands.RevokeSession.V1;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Session.UseCases.Public.Commands.RevokeSession.V1;

/// <summary>
/// Integration tests for the PublicRevokeSession endpoint.
/// </summary>
[Collection("Database")]
public class PublicRevokeSessionEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string RevokeSessionBaseUrl =
        $"{ApiRoutes.Public.Me}/{SessionRouteConstants.Endpoint}/{SessionRouteConstants.Revoke}";

    [Fact]
    public async Task PublicRevokeSession_WithNoAuth_Returns401()
    {
        Client.ClearAuthentication();

        Guid nonExistentId = Guid.NewGuid();
        var response = await Client.PostAsync($"{RevokeSessionBaseUrl}/{nonExistentId}", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RevokeSession_AsVisitor_WithOwnSession_ReturnsOk()
    {
        Guid sessionId = Guid.NewGuid();
        await SeedAsync<IdentityDbContext>(ctx =>
        {
            SessionEntity session = SessionFactory.CreateWithId(sessionId, TestUser.VisitorId);
            ctx.Sessions.Add(session);
        });

        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsync($"{RevokeSessionBaseUrl}/{sessionId}", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicRevokeSessionResponse body = await response.ReadAsAsync<PublicRevokeSessionResponse>();
        body.IsSuccess.Should().BeTrue();

        await using IdentityDbContext context = CreateDbContext<IdentityDbContext>();
        SessionEntity? persisted = await context.Sessions.FirstOrDefaultAsync(s => s.Id == sessionId);
        persisted.Should().NotBeNull();
        persisted!.IsRevoked.Should().BeTrue();
    }

    [Fact]
    public async Task RevokeSession_WithOtherUsersSession_ReturnsNotFound()
    {
        Guid otherSessionId = Guid.NewGuid();
        await SeedAsync<IdentityDbContext>(ctx =>
        {
            UserEntity otherUser = UserFactory.CreateVerifiedActive();
            SessionEntity otherSession = SessionFactory.CreateWithId(otherSessionId, otherUser.Id);
            ctx.Users.Add(otherUser);
            ctx.Sessions.Add(otherSession);
        });

        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsync($"{RevokeSessionBaseUrl}/{otherSessionId}", null);

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Session"))
        );
    }

    [Fact]
    public async Task RevokeSession_WithNonExistentSession_ReturnsNotFound()
    {
        Client.AuthenticateAsVisitor();

        Guid nonExistentId = Guid.NewGuid();
        var response = await Client.PostAsync($"{RevokeSessionBaseUrl}/{nonExistentId}", null);

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Session"))
        );
    }
}
