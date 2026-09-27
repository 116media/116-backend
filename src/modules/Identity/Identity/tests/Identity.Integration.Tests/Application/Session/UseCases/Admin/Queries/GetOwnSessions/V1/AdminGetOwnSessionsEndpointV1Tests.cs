using _116.Identity.Application.Session.Constants;
using _116.Identity.Application.Session.UseCases.Admin.Queries.GetOwnSessions.V1;
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

namespace _116.Identity.Integration.Tests.Application.Session.UseCases.Admin.Queries.GetOwnSessions.V1;

/// <summary>
/// Integration tests for the AdminGetOwnSessions endpoint.
/// </summary>
[Collection("Database")]
public class AdminGetOwnSessionsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string AdminMeSessions =
        $"{ApiRoutes.Admin.Base}/{IdentityConstants.Me}/{SessionRouteConstants.Endpoint}";

    [Fact]
    public async Task AdminGetOwnSessions_AsSuperAdmin_Returns200()
    {
        SessionEntity session = await SeedAsync<IdentityDbContext, SessionEntity>(ctx =>
        {
            SessionEntity entity = SessionFactory.Create(TestUser.SuperAdminId);
            ctx.Sessions.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync(AdminMeSessions);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminGetOwnSessionsResponse body = await response.ReadAsAsync<AdminGetOwnSessionsResponse>();
        body.Sessions.Should().Contain(s => s.Id == session.Id);
    }

    [Fact]
    public async Task AdminGetOwnSessions_WithNoAuth_Returns401()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync(AdminMeSessions);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
