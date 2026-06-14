using System.Text.Json;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.User.UseCases.Admin.Commands.AssignRoleToUser.V1;

/// <summary>
/// Integration tests for the AdminAssignRoleToUser endpoint.
/// </summary>
[Collection("Database")]
public class AdminAssignRoleToUserEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string AdminMeProfile = $"{ApiRoutes.Admin.Base}/me/profile";
    private const string AdminMeAvatar = $"{ApiRoutes.Admin.Base}/me/avatar";
    private const string PublicMeProfile = $"{ApiRoutes.Public.Me}/profile";
    private const string PublicMeAvatar = $"{ApiRoutes.Public.Me}/avatar";

    [Fact]
    public async Task AdminAssignRole_AsSuperAdmin_WithValidData_Returns200()
    {
        await using var context = CreateDbContext<IdentityDbContext>();
        var role = RoleFactory.Create();
        context.Roles.Add(role);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();
        var request = new { RoleId = role.Id };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Admin.Users}/{TestUser.AdminId}/roles", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AdminAssignRole_AsAdmin_Returns403()
    {
        await using var context = CreateDbContext<IdentityDbContext>();
        var role = RoleFactory.Create();
        context.Roles.Add(role);
        await context.SaveChangesAsync();

        Client.AuthenticateAsAdmin();
        var request = new { RoleId = role.Id };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Admin.Users}/{TestUser.AdminId}/roles", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminAssignRole_WithNonExistentRole_Returns404()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new { RoleId = Guid.NewGuid() };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Admin.Users}/{TestUser.AdminId}/roles", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
