using _116.Identity.Application.Session.UseCases.Admin.Commands.ForceLogoutUser.V1;
using _116.Identity.Domain.Entities;
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
        Guid userId = Guid.NewGuid();

        var response = await Client.PostAsync(Routes.Admin.Sessions.ForceLogout(userId), null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ForceLogoutUser_AsSuperAdmin_WithNonExistentUser_ReturnsOk()
    {
        Client.AuthenticateAsSuperAdmin();
        Guid nonExistentId = Guid.NewGuid();

        var response = await Client.PostAsync(Routes.Admin.Sessions.ForceLogout(nonExistentId), null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminForceLogoutUserResponse body = await response.ReadAsAsync<AdminForceLogoutUserResponse>();
        body.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ForceLogoutUser_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();
        Guid userId = Guid.NewGuid();

        var response = await Client.PostAsync(Routes.Admin.Sessions.ForceLogout(userId), null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ForceLogoutUser_AsSuperAdmin_WithExistingUserSessions_ReturnsOk()
    {
        List<SessionEntity> sessions = SessionFactory.CreateMany(TestUser.VisitorId, 3);
        await SeedAsync<IdentityDbContext>(ctx =>
        {
            ctx.Sessions.AddRange(sessions);
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PostAsync(Routes.Admin.Sessions.ForceLogout(TestUser.VisitorId), null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminForceLogoutUserResponse body = await response.ReadAsAsync<AdminForceLogoutUserResponse>();
        body.IsSuccess.Should().BeTrue();

        await using IdentityDbContext context = CreateDbContext<IdentityDbContext>();
        List<SessionEntity> persisted = await context.Sessions.Where(s => s.UserId == TestUser.VisitorId).ToListAsync();
        persisted.Should().NotBeEmpty();
        persisted.Should().OnlyContain(s => s.IsRevoked);
    }
}
