using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.UpdateRole.V1;

/// <summary>
/// Integration tests for the AdminUpdateRole endpoint.
/// </summary>
[Collection("Database")]
public class AdminUpdateRoleEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ShortName(string prefix = "r") => $"{prefix}{Guid.NewGuid().ToString("N")[..8]}";

    [Fact]
    public async Task UpdateRole_AsSuperAdmin_WithValidData_ReturnsSuccess()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var role = RoleFactory.Create(ShortName("ub"), "Original description");
        seedContext.Roles.Add(role);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();
        var request = new { Name = ShortName("ua"), Description = "Updated description" };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Roles}/{role.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateRole_NonExistentRole_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new { Name = ShortName("gh"), Description = "Does not exist" };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Roles}/{Guid.NewGuid()}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateRole_AsAdmin_ReturnsForbidden()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var role = RoleFactory.Create(ShortName("uf"), "Admin cannot update");
        seedContext.Roles.Add(role);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsAdmin();
        var request = new { Name = ShortName("ux"), Description = "Should be forbidden" };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Roles}/{role.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
