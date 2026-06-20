using System.Text.Json;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Session.UseCases.Admin.Queries.GetAllSessions.V1;

/// <summary>
/// Integration tests for the AdminGetAllSessions endpoint.
/// </summary>
[Collection("Database")]
public class AdminGetAllSessionsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task AdminGetAllSessions_AsSuperAdmin_Returns200()
    {
        await using var context = CreateDbContext<IdentityDbContext>();
        var session = SessionFactory.Create(TestUser.SuperAdminId);
        context.Sessions.Add(session);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Sessions}?pageIndex=0&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        root.TryGetProperty("sessions", out var sessionsProp).Should().BeTrue();
        sessionsProp.TryGetProperty("items", out var items).Should().BeTrue();
        items.GetArrayLength().Should().BeGreaterThanOrEqualTo(1);
        sessionsProp.TryGetProperty("pageIndex", out _).Should().BeTrue();
        sessionsProp.TryGetProperty("pageSize", out _).Should().BeTrue();
        sessionsProp.TryGetProperty("count", out _).Should().BeTrue();
    }

    [Fact]
    public async Task AdminGetAllSessions_WithNoAuth_Returns401()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Sessions}?pageIndex=0&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AdminGetAllSessions_AsVisitor_Returns403()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Sessions}?pageIndex=0&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
