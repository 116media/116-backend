using System.Text.Json;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Session.UseCases.Admin.Commands.RevokeSession.V1;

/// <summary>
/// Integration tests for the AdminRevokeSession endpoint.
/// </summary>
[Collection("Database")]
public class AdminRevokeSessionEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task AdminRevokeSession_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PostAsync($"/api/v1/admin/me/sessions/revoke/{Guid.NewGuid()}", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AdminRevokeSession_WithNonExistentId_Returns404()
    {
        Client.AuthenticateAsSuperAdmin();

        var nonExistentId = Guid.NewGuid();
        var response = await Client.PostAsync($"/api/v1/admin/me/sessions/revoke/{nonExistentId}", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AdminRevokeSession_AsSuperAdmin_WithExistingSession_ReturnsOk()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var session = SessionFactory.Create(TestUser.SuperAdminId);
        seedContext.Sessions.Add(session);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PostAsync($"/api/v1/admin/me/sessions/revoke/{session.Id}", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
