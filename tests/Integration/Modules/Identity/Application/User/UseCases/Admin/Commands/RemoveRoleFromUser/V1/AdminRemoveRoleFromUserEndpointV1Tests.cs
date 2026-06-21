using System.Text.Json;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.User.UseCases.Admin.Commands.RemoveRoleFromUser.V1;

/// <summary>
/// Integration tests for the AdminRemoveRoleFromUser endpoint.
/// </summary>
[Collection("Database")]
public class AdminRemoveRoleFromUserEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string AdminMeProfile = $"{ApiRoutes.Admin.Base}/me/profile";
    private const string AdminMeAvatar = $"{ApiRoutes.Admin.Base}/me/avatar";
    private const string PublicMeProfile = $"{ApiRoutes.Public.Me}/profile";
    private const string PublicMeAvatar = $"{ApiRoutes.Public.Me}/avatar";

    [Fact]
    public async Task AdminRemoveRole_AsSuperAdmin_Returns200()
    {
        await using var context = CreateDbContext<IdentityDbContext>();
        var role = RoleFactory.Create();
        context.Roles.Add(role);
        await context.SaveChangesAsync();

        var userRole = UserRoleFactory.Create(TestUser.AdminId, role.Id);
        context.UserRoles.Add(userRole);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Users}/{TestUser.AdminId}/roles/{role.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AdminRemoveRole_AsAdmin_Returns403()
    {
        await using var context = CreateDbContext<IdentityDbContext>();
        var role = RoleFactory.Create();
        context.Roles.Add(role);
        await context.SaveChangesAsync();

        var userRole = UserRoleFactory.Create(TestUser.AdminId, role.Id);
        context.UserRoles.Add(userRole);
        await context.SaveChangesAsync();

        Client.AuthenticateAsAdmin();

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Users}/{TestUser.AdminId}/roles/{role.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>
    /// Verifies that removing a role that is not assigned to the user returns a 400 Bad Request.
    /// </summary>
    [Fact]
    public async Task RemoveRole_WhenNotAssigned_ReturnsBadRequest()
    {
        await using var context = CreateDbContext<IdentityDbContext>();
        var role = RoleFactory.Create();
        context.Roles.Add(role);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Users}/{TestUser.AdminId}/roles/{role.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
