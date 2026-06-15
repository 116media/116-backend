using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.HardDeleteRole.V1;

/// <summary>
/// Integration tests for the AdminHardDeleteRole endpoint.
/// </summary>
[Collection("Database")]
public class AdminHardDeleteRoleEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ShortName(string prefix = "r") => $"{prefix}{Guid.NewGuid().ToString("N")[..8]}";

    [Fact]
    public async Task HardDeleteRole_AsSuperAdmin_ReturnsSuccess_AndRemovesFromDatabase()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var role = RoleFactory.Create(ShortName("hd"), "Will be permanently deleted");
        seedContext.Roles.Add(role);
        await seedContext.SaveChangesAsync();
        var roleId = role.Id;

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Roles}/{roleId}/hard");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        var deleted = await verifyContext.Roles.FirstOrDefaultAsync(r => r.Id == roleId);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task HardDeleteRole_AsVisitor_ReturnsForbidden()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var role = RoleFactory.Create(ShortName("hf"), "Visitor cannot delete");
        seedContext.Roles.Add(role);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsVisitor();

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Roles}/{role.Id}/hard");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>
    /// Verifies that hard-deleting a core role (SuperAdmin, Admin, or Visitor) returns a 400 Bad Request.
    /// </summary>
    [Fact]
    public async Task HardDeleteRole_CoreRole_ReturnsBadRequest()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var coreRole = RoleFactory.CreateSuperAdmin();
        seedContext.Roles.Add(coreRole);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Roles}/{coreRole.Id}/hard");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
