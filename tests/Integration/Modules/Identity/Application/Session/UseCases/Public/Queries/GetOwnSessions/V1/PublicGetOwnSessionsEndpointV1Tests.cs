using System.Text.Json;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Session.UseCases.Public.Queries.GetOwnSessions.V1;

/// <summary>
/// Integration tests for the PublicGetOwnSessions endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetOwnSessionsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task PublicGetOwnSessions_AsVisitor_Returns200()
    {
        await using var context = CreateDbContext<IdentityDbContext>();
        var session = SessionFactory.Create(TestUser.VisitorId);
        context.Sessions.Add(session);
        await context.SaveChangesAsync();

        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Me}/sessions");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        root.TryGetProperty("sessions", out var sessionsProp).Should().BeTrue();
        sessionsProp.ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    public async Task PublicGetOwnSessions_WithNoAuth_Returns401()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Me}/sessions");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PublicGetOwnSessions_AsAdmin_Returns403()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Me}/sessions");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
