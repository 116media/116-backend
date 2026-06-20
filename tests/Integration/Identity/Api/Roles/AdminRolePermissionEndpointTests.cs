using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Identity.Api.Roles;

/// <summary>
/// Integration tests for the admin role-permission assignment endpoints verifying
/// assign, remove, and bulk-update operations against a real PostgreSQL database
/// through the full API pipeline.
/// </summary>
[Collection("Database")]
public class AdminRolePermissionEndpointTests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task AssignPermission_AsSuperAdmin_WithValidData_ReturnsSuccess()
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

        Client.AuthenticateAsSuperAdmin();
        var request = new { PermissionId = permissionId };

        // Act
        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Admin.Roles}/{roleId}/permissions", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        var assignment = await verifyContext.RolePermissions.FirstOrDefaultAsync(rp =>
            rp.RoleId == roleId && rp.PermissionId == permissionId
        );
        assignment.Should().NotBeNull();
    }

    [Fact]
    public async Task AssignPermission_AsAdmin_ReturnsForbidden()
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

        Client.AuthenticateAsAdmin();
        var request = new { PermissionId = permissionId };

        // Act
        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Admin.Roles}/{roleId}/permissions", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AssignPermission_WithNoAuth_ReturnsUnauthorized()
    {
        // Arrange
        Client.ClearAuthentication();
        var roleId = Guid.NewGuid();
        var request = new { PermissionId = Guid.NewGuid() };

        // Act
        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Admin.Roles}/{roleId}/permissions", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AssignPermission_NonExistentRole_ReturnsNotFound()
    {
        // Arrange
        var permissionId = Guid.NewGuid();

        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var permission = PermissionFactory.CreateWithId(permissionId, $"rpa_{Guid.NewGuid():N}"[..15], "read");
        seedContext.Permissions.Add(permission);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();
        var nonExistentRoleId = Guid.NewGuid();
        var request = new { PermissionId = permissionId };

        // Act
        var response = await Client.PostAsJsonAsync(
            $"{ApiRoutes.Admin.Roles}/{nonExistentRoleId}/permissions",
            request
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AssignPermission_NonExistentPermission_ReturnsNotFound()
    {
        // Arrange
        var roleId = Guid.NewGuid();

        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var role = RoleFactory.CreateWithId(roleId, $"rpa_{Guid.NewGuid():N}"[..20]);
        seedContext.Roles.Add(role);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();
        var nonExistentPermissionId = Guid.NewGuid();
        var request = new { PermissionId = nonExistentPermissionId };

        // Act
        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Admin.Roles}/{roleId}/permissions", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AssignPermission_AlreadyAssigned_ReturnsConflict()
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
        var request = new { PermissionId = permissionId };

        // Act
        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Admin.Roles}/{roleId}/permissions", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

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
