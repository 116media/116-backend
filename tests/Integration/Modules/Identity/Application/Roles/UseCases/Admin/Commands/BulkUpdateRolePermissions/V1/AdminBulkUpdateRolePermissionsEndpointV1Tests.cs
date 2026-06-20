using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.BulkUpdateRolePermissions.V1;

/// <summary>
/// Integration tests for the AdminBulkUpdateRolePermissions endpoint.
/// </summary>
[Collection("Database")]
public class AdminBulkUpdateRolePermissionsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task BulkUpdatePermissions_AsSuperAdmin_WithValidList_ReturnsSuccess()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var permissionId1 = Guid.NewGuid();
        var permissionId2 = Guid.NewGuid();
        var permissionId3 = Guid.NewGuid();

        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var role = RoleFactory.CreateWithId(roleId, $"rpa_{Guid.NewGuid():N}"[..20]);
        var perm1 = PermissionFactory.CreateWithId(permissionId1, $"rpa_{Guid.NewGuid():N}"[..15], "read");
        var perm2 = PermissionFactory.CreateWithId(permissionId2, $"rpa_{Guid.NewGuid():N}"[..15], "create");
        var perm3 = PermissionFactory.CreateWithId(permissionId3, $"rpa_{Guid.NewGuid():N}"[..15], "update");
        seedContext.Roles.Add(role);
        seedContext.Permissions.AddRange(perm1, perm2, perm3);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();
        var request = new { PermissionIds = new List<Guid> { permissionId1, permissionId2, permissionId3 } };

        // Act
        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Roles}/{roleId}/permissions", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        var assignments = await verifyContext.RolePermissions.Where(rp => rp.RoleId == roleId).ToListAsync();
        assignments.Should().HaveCount(3);
        assignments.Select(a => a.PermissionId).Should().BeEquivalentTo([permissionId1, permissionId2, permissionId3]);
    }

    [Fact]
    public async Task BulkUpdatePermissions_AsVisitor_ReturnsForbidden()
    {
        // Arrange
        var roleId = Guid.NewGuid();

        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var role = RoleFactory.CreateWithId(roleId, $"rpa_{Guid.NewGuid():N}"[..20]);
        seedContext.Roles.Add(role);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsVisitor();
        var request = new { PermissionIds = new List<Guid> { Guid.NewGuid() } };

        // Act
        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Roles}/{roleId}/permissions", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
