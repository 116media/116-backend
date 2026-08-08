using _116.Identity.Application.Session.Constants;
using _116.Identity.Application.Session.UseCases.Public.Queries.GetOwnSessionById.V1;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Session.UseCases.Public.Queries.GetOwnSessionById.V1;

/// <summary>
/// Integration tests for the PublicGetOwnSessionById endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetOwnSessionByIdEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string PublicMeSessions = $"{ApiRoutes.Public.Me}/{SessionRouteConstants.Endpoint}";

    [Fact]
    public async Task GetOwnSessionById_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        Guid sessionId = Guid.NewGuid();

        var response = await Client.GetAsync($"{PublicMeSessions}/{sessionId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetOwnSessionById_AsVisitor_WithNonExistentId_ReturnsNotFound()
    {
        Client.AuthenticateAsVisitor();
        Guid nonExistentId = Guid.NewGuid();

        var response = await Client.GetAsync($"{PublicMeSessions}/{nonExistentId}");

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Session"))
        );
    }

    [Fact]
    public async Task GetOwnSessionById_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();
        Guid sessionId = Guid.NewGuid();

        var response = await Client.GetAsync($"{PublicMeSessions}/{sessionId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetOwnSessionById_AsVisitor_WithOwnSession_ReturnsOk()
    {
        SessionEntity session = await SeedAsync<IdentityDbContext, SessionEntity>(ctx =>
        {
            SessionEntity entity = SessionFactory.Create(TestUser.VisitorId);
            ctx.Sessions.Add(entity);
            return entity;
        });

        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync($"{PublicMeSessions}/{session.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicGetOwnSessionByIdResponse body = await response.ReadAsAsync<PublicGetOwnSessionByIdResponse>();
        body.Session.Id.Should().Be(session.Id);
    }
}
