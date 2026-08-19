using _116.Identity.Application.Session.Constants;
using _116.Identity.Application.Session.UseCases.Public.Queries.GetOwnSessions.V1;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Session.UseCases.Public.Queries.GetOwnSessions.V1;

/// <summary>
/// Integration tests for the PublicGetOwnSessions endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetOwnSessionsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string PublicMeSessions = $"{ApiRoutes.Public.Me}/{SessionRouteConstants.Endpoint}";

    [Fact]
    public async Task PublicGetOwnSessions_AsVisitor_Returns200()
    {
        SessionEntity session = await SeedAsync<IdentityDbContext, SessionEntity>(ctx =>
        {
            SessionEntity entity = SessionFactory.Create(TestUser.VisitorId);
            ctx.Sessions.Add(entity);
            return entity;
        });

        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync(PublicMeSessions);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicGetOwnSessionsResponse body = await response.ReadAsAsync<PublicGetOwnSessionsResponse>();
        body.Sessions.Should().Contain(s => s.Id == session.Id);
    }

    [Fact]
    public async Task PublicGetOwnSessions_WithNoAuth_Returns401()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync(PublicMeSessions);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PublicGetOwnSessions_AsAdmin_Returns403()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.GetAsync(PublicMeSessions);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>
    /// The listing marks the caller's own session, so a token carrying no session claim cannot be
    /// served: the account passes the status policy but the request has no identifiable session to
    /// mark. The credential is rejected instead of the response guessing one.
    /// </summary>
    [Fact]
    public async Task PublicGetOwnSessions_WithATokenCarryingNoSessionClaim_Returns401()
    {
        await SeedAsync<IdentityDbContext>(ctx => ctx.Sessions.Add(SessionFactory.Create(TestUser.VisitorId)));

        Client.AuthenticateWithoutSessionClaim(TestUser.VisitorId, "Visitor");

        var response = await Client.GetAsync(PublicMeSessions);

        await response.ShouldBeProblem(HttpStatusCode.Unauthorized);
    }
}
