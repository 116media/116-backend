using System.Text.Json;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Session.UseCases.Admin.Queries.GetOwnSessions.V1;

/// <summary>
/// Integration tests for the AdminGetOwnSessions endpoint.
/// </summary>
[Collection("Database")]
public class AdminGetOwnSessionsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task AdminGetOwnSessions_AsSuperAdmin_Returns200()
    {
        await using var context = CreateDbContext<IdentityDbContext>();
        var session = SessionFactory.Create(TestUser.SuperAdminId);
        context.Sessions.Add(session);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync("/api/v1/admin/me/sessions");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        root.TryGetProperty("sessions", out var sessionsProp).Should().BeTrue();
        sessionsProp.ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    public async Task AdminGetOwnSessions_WithNoAuth_Returns401()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync("/api/v1/admin/me/sessions");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
