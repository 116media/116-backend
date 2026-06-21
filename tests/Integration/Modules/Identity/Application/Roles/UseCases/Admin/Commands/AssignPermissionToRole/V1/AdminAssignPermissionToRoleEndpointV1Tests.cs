using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.AssignPermissionToRole.V1;

/// <summary>
/// Integration tests for the AdminAssignPermissionToRole endpoint.
/// </summary>
[Collection("Database")]
public class AdminAssignPermissionToRoleEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
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

    /// <summary>
    /// Verifies that assigning an inactive permission to a role returns a 400 Bad Request.
    /// </summary>
    [Fact]
    public async Task AssignPermission_WhenPermissionInactive_ReturnsBadRequest()
    {
        var roleId = Guid.NewGuid();

        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var role = RoleFactory.CreateWithId(roleId, $"rpa_{Guid.NewGuid():N}"[..20]);
        var permission = PermissionFactory.CreateInactive();
        seedContext.Roles.Add(role);
        seedContext.Permissions.Add(permission);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();
        var request = new { PermissionId = permission.Id };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Admin.Roles}/{roleId}/permissions", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Verifies that assigning a soft-deleted permission to a role returns a 400 Bad Request.
    /// </summary>
    [Fact]
    public async Task AssignPermission_WhenPermissionDeleted_ReturnsBadRequest()
    {
        var roleId = Guid.NewGuid();

        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var role = RoleFactory.CreateWithId(roleId, $"rpa_{Guid.NewGuid():N}"[..20]);
        var permission = PermissionFactory.CreateDeleted();
        seedContext.Roles.Add(role);
        seedContext.Permissions.Add(permission);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();
        var request = new { PermissionId = permission.Id };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Admin.Roles}/{roleId}/permissions", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
