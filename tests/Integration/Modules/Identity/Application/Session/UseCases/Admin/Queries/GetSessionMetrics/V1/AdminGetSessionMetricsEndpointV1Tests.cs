using _116.Identity.Application.Session.UseCases.Admin.Queries.GetSessionMetrics.V1;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Session.UseCases.Admin.Queries.GetSessionMetrics.V1;

/// <summary>
/// Integration tests for the AdminGetSessionMetrics endpoint.
/// </summary>
[Collection("Database")]
public class AdminGetSessionMetricsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task AdminGetSessionMetrics_AsSuperAdmin_Returns200()
    {
        await SeedAsync<IdentityDbContext>(ctx =>
        {
            SessionEntity session = SessionFactory.CreateDesktop(TestUser.SuperAdminId);
            ctx.Sessions.Add(session);
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync(Routes.Admin.Sessions.Metrics());

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminGetSessionMetricsResponse body = await response.ReadAsAsync<AdminGetSessionMetricsResponse>();
        body.Browsers.Should().NotBeNull();
        body.Devices.Should().NotBeNull();
        body.Platforms.Should().NotBeNull();
        body.Clients.Should().NotBeNull();
        body.TotalActiveSessions.Should().Be(1);
        body.TotalActiveUsers.Should().Be(1);
    }

    [Fact]
    public async Task AdminGetSessionMetrics_AsVisitor_Returns403()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync(Routes.Admin.Sessions.Metrics());

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
