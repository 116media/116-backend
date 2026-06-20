using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Auth.UseCases.Admin.Commands.SignOutFromAllDevices.V1;

/// <summary>
/// Integration tests for the AdminSignOutFromAllDevices endpoint.
/// </summary>
[Collection("Database")]
public class AdminSignOutFromAllDevicesEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string AuthUrl = ApiRoutes.Admin.Auth;

    [Fact]
    public async Task SignOutFromAllDevices_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PostAsync($"{AuthUrl}/sign-out-all", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SignOutFromAllDevices_AsSuperAdmin_ReturnsOk()
    {
        await using var context = CreateDbContext<IdentityDbContext>();
        var session = SessionFactory.Create(TestUser.SuperAdminId);
        context.Sessions.Add(session);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PostAsync($"{AuthUrl}/sign-out-all", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
