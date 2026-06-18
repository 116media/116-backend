using _116.Identity.Application.Roles.Constants;
using _116.Identity.Application.User.UseCases.Admin.Queries.GetUserRoles.V1;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.User.UseCases.Admin.Queries.GetUserRoles.V1;

/// <summary>
/// Integration tests for the AdminGetUserRoles endpoint.
/// </summary>
[Collection("Database")]
public class AdminGetUserRolesEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string UserRolesUrl(Guid userId) =>
        $"{ApiRoutes.Admin.Users}/{userId}/{RoleRouteConstants.Endpoint}";

    [Fact]
    public async Task AdminGetUserRoles_AsSuperAdmin_Returns200()
    {
        RoleEntity role = await SeedAsync<IdentityDbContext, RoleEntity>(context =>
        {
            RoleEntity created = RoleFactory.Create();
            context.Roles.Add(created);
            context.UserRoles.Add(UserRoleFactory.Create(TestUser.AdminId, created.Id));
            return created;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync(UserRolesUrl(TestUser.AdminId));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminGetUserRolesResponse body = await response.ReadAsAsync<AdminGetUserRolesResponse>();
        body.Roles.Should().Contain(r => r.Id == role.Id && r.Name == role.Name);
    }

    [Fact]
    public async Task AdminGetUserRoles_WithNoAuth_Returns401()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync(UserRolesUrl(TestUser.AdminId));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AdminGetUserRoles_AsVisitor_Returns403()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync(UserRolesUrl(TestUser.AdminId));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
