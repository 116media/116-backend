using _116.Identity.Application.Auth.UseCases.Public.Commands.SignOutFromAllDevices.V1;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.SignOutFromAllDevices.V1;

/// <summary>
/// Integration tests for the PublicSignOutFromAllDevices endpoint.
/// </summary>
[Collection("Database")]
public class PublicSignOutFromAllDevicesEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task SignOutFromAllDevices_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PostAsync(Routes.Public.Auth.SignOutAll(), null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SignOutFromAllDevices_AsVisitor_ReturnsOk()
    {
        await using var context = CreateDbContext<IdentityDbContext>();
        var session = SessionFactory.Create(TestUser.VisitorId);
        context.Sessions.Add(session);
        await context.SaveChangesAsync();

        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsync(Routes.Public.Auth.SignOutAll(), null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicSignOutFromAllDevicesResponse body = await response.ReadAsAsync<PublicSignOutFromAllDevicesResponse>();
        body.IsSuccess.Should().BeTrue();

        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        var sessions = await verifyContext.Sessions.Where(s => s.UserId == TestUser.VisitorId).ToListAsync();
        sessions.Should().NotBeEmpty();
        sessions.Should().OnlyContain(s => s.IsRevoked);
    }

    [Fact]
    public async Task SignOutFromAllDevices_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.PostAsync(Routes.Public.Auth.SignOutAll(), null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
