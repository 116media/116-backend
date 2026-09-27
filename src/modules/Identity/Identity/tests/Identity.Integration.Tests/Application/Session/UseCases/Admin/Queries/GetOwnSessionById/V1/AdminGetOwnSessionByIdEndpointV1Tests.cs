using _116.BuildingBlocks.Application.Exceptions;
using _116.BuildingBlocks.Application.Exceptions.Messages;
using _116.Identity.Application.Session.Constants;
using _116.Identity.Application.Session.UseCases.Admin.Queries.GetOwnSessionById.V1;
using _116.Identity.Domain.Constants;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Identity.TestData.Mocks.Services;
using _116.Storage.TestData.Factories;
using _116.Storage.TestData.Mocks.Infrastructure;
using _116.Storage.TestData.Mocks.Services;
using _116.Tests.TestData.Mocks;

namespace _116.Identity.Integration.Tests.Application.Session.UseCases.Admin.Queries.GetOwnSessionById.V1;

/// <summary>
/// Integration tests for the AdminGetOwnSessionById endpoint.
/// </summary>
[Collection("Database")]
public class AdminGetOwnSessionByIdEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string AdminMeSessions =
        $"{ApiRoutes.Admin.Base}/{IdentityConstants.Me}/{SessionRouteConstants.Endpoint}";

    [Fact]
    public async Task GetOwnSessionById_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        Guid sessionId = Guid.NewGuid();

        var response = await Client.GetAsync($"{AdminMeSessions}/{sessionId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetOwnSessionById_AsSuperAdmin_WithNonExistentId_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();
        Guid nonExistentId = Guid.NewGuid();

        var response = await Client.GetAsync($"{AdminMeSessions}/{nonExistentId}");

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Session"))
        );
    }

    [Fact]
    public async Task GetOwnSessionById_AsSuperAdmin_WithExistingSession_ReturnsOk()
    {
        SessionEntity session = await SeedAsync<IdentityDbContext, SessionEntity>(ctx =>
        {
            SessionEntity entity = SessionFactory.Create(TestUser.SuperAdminId);
            ctx.Sessions.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{AdminMeSessions}/{session.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminGetOwnSessionByIdResponse body = await response.ReadAsAsync<AdminGetOwnSessionByIdResponse>();
        body.Session.Id.Should().Be(session.Id);
    }
}
