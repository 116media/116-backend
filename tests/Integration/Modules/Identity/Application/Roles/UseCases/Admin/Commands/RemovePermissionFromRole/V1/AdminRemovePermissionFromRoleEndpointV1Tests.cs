using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.RemovePermissionFromRole.V1;

/// <summary>
/// Integration tests for the AdminRemovePermissionFromRole endpoint.
/// </summary>
[Collection("Database")]
public class AdminRemovePermissionFromRoleEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task RemovePermission_AsSuperAdmin_ExistingAssignment_ReturnsSuccess()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();

        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var role = RoleFactory.CreateWithId(roleId, $"rpa_{Guid.NewGuid():N}"[..20]);
        var permission = PermissionFactory.CreateWithId(permissionId, $"rpa_{Guid.NewGuid():N}"[..15], "read");
        seedContext.Roles.Add(role);
        seedContext.Permissions.Add(permission);
        await seedContext.SaveChangesAsync();

        var rolePermission = RolePermissionEntity.Create(Guid.NewGuid(), roleId, permissionId);
        seedContext.RolePermissions.Add(rolePermission);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        // Act
        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Roles}/{roleId}/permissions/{permissionId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        var assignment = await verifyContext.RolePermissions.FirstOrDefaultAsync(rp =>
            rp.RoleId == roleId && rp.PermissionId == permissionId
        );
        assignment.Should().BeNull();
    }

    [Fact]
    public async Task RemovePermission_NonExistentAssignment_ReturnsBadRequest()
    {
        var roleId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();

        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var role = RoleFactory.CreateWithId(roleId, $"rpa_{Guid.NewGuid():N}"[..20]);
        var permission = PermissionFactory.CreateWithId(permissionId, $"rpa_{Guid.NewGuid():N}"[..15], "read");
        seedContext.Roles.Add(role);
        seedContext.Permissions.Add(permission);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Roles}/{roleId}/permissions/{permissionId}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
