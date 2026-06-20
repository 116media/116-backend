using System.Text.Json;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Session.UseCases.Admin.Commands.CleanupExpiredSessions.V1;

/// <summary>
/// Integration tests for the AdminCleanupExpiredSessions endpoint.
/// </summary>
[Collection("Database")]
public class AdminCleanupExpiredSessionsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task AdminCleanup_AsSuperAdmin_Returns200()
    {
        await using var context = CreateDbContext<IdentityDbContext>();
        var expiredSession = SessionFactory.CreateExpired(TestUser.SuperAdminId);
        context.Sessions.Add(expiredSession);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PostAsync($"{ApiRoutes.Admin.Sessions}/cleanup", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        root.TryGetProperty("deletedCount", out var deletedCount).Should().BeTrue();
        deletedCount.GetInt32().Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task AdminCleanup_AsAdmin_Returns403()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.PostAsync($"{ApiRoutes.Admin.Sessions}/cleanup", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
