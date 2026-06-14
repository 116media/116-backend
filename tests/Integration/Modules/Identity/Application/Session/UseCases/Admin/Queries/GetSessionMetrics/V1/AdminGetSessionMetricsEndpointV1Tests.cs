using System.Text.Json;
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
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Sessions}/metrics");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AdminGetSessionMetrics_AsVisitor_Returns403()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Sessions}/metrics");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
