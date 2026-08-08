using _116.Identity.Application.Roles.UseCases.Admin.Queries.GetOwnRoles.V1;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Queries.GetOwnRoles.V1;

/// <summary>
/// Integration tests for the AdminGetOwnRoles endpoint.
/// </summary>
[Collection("Database")]
public class AdminGetOwnRolesEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ShortName(string prefix = "rq") => $"{prefix}{Guid.NewGuid().ToString("N")[..8]}";

    [Fact]
    public async Task GetOwnRoles_AsSuperAdminWithSeededRoles_ReturnsOk()
    {
        RoleEntity role = await SeedAsync<IdentityDbContext, RoleEntity>(ctx =>
        {
            RoleEntity entity = RoleFactory.Create(ShortName("own"), "Role assigned to SuperAdmin.");
            ctx.Roles.Add(entity);
            ctx.UserRoles.Add(UserRoleFactory.Create(TestUser.SuperAdminId, entity.Id));
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Base}/me/roles");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminGetOwnRolesResponse>();
        body.Roles.Should().Contain(r => r.Id == role.Id);
    }

    [Fact]
    public async Task GetOwnRoles_WithoutAuthentication_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Base}/me/roles");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetOwnRoles_WithInactiveAccount_ReturnsForbidden()
    {
        UserEntity inactiveUser = await SeedAsync<IdentityDbContext, UserEntity>(ctx =>
        {
            UserEntity entity = UserFactory.CreateInactive();
            ctx.Users.Add(entity);
            return entity;
        });

        Client.AuthenticateAs(inactiveUser.Id, "SuperAdmin");

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Base}/me/roles");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
