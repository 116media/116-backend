using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Session.UseCases.Admin.Commands.ForceLogoutUser.V1;

/// <summary>
/// Integration tests for the AdminForceLogoutUser endpoint.
/// </summary>
[Collection("Database")]
public class AdminForceLogoutUserEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task ForceLogoutUser_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var userId = Guid.NewGuid();

        var response = await Client.PostAsync($"{ApiRoutes.Admin.Sessions}/force-logout/{userId}", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ForceLogoutUser_AsSuperAdmin_WithNonExistentUser_ReturnsOk()
    {
        Client.AuthenticateAsSuperAdmin();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PostAsync($"{ApiRoutes.Admin.Sessions}/force-logout/{nonExistentId}", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ForceLogoutUser_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();
        var userId = Guid.NewGuid();

        var response = await Client.PostAsync($"{ApiRoutes.Admin.Sessions}/force-logout/{userId}", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ForceLogoutUser_AsSuperAdmin_WithExistingUserSessions_ReturnsOk()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var sessions = SessionFactory.CreateMany(TestUser.VisitorId, 3);
        seedContext.Sessions.AddRange(sessions);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PostAsync($"{ApiRoutes.Admin.Sessions}/force-logout/{TestUser.VisitorId}", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
