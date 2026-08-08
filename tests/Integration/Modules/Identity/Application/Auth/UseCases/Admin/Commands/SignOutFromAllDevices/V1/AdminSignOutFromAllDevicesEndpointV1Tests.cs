using _116.Identity.Application.Auth.Constants;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.SignOutFromAllDevices.V1;
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
    private const string SignOutAllUrl = $"{AuthUrl}/{AuthRouteConstants.SignOutAll}";

    [Fact]
    public async Task SignOutFromAllDevices_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PostAsync(SignOutAllUrl, null);

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

        var response = await Client.PostAsync(SignOutAllUrl, null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminSignOutFromAllDevicesResponse body = await response.ReadAsAsync<AdminSignOutFromAllDevicesResponse>();
        body.IsSuccess.Should().BeTrue();

        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        var sessions = await verifyContext.Sessions.Where(s => s.UserId == TestUser.SuperAdminId).ToListAsync();
        sessions.Should().NotBeEmpty();
        sessions.Should().OnlyContain(s => s.IsRevoked);
    }
}
