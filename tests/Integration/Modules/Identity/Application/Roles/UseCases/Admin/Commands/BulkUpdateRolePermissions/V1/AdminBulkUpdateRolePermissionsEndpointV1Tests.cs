using _116.Identity.Application.Roles.UseCases.Admin.Commands.BulkUpdateRolePermissions.V1;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Builders.Requests.Identity;
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
        Guid roleId = Guid.NewGuid();
        Guid permissionId1 = Guid.NewGuid();
        Guid permissionId2 = Guid.NewGuid();
        Guid permissionId3 = Guid.NewGuid();

        await SeedAsync<IdentityDbContext>(ctx =>
        {
            ctx.Roles.Add(RoleFactory.CreateWithId(roleId, $"rpa_{Guid.NewGuid():N}"[..20]));
            ctx.Permissions.AddRange(
                PermissionFactory.CreateWithId(permissionId1, $"rpa_{Guid.NewGuid():N}"[..15], "read"),
                PermissionFactory.CreateWithId(permissionId2, $"rpa_{Guid.NewGuid():N}"[..15], "create"),
                PermissionFactory.CreateWithId(permissionId3, $"rpa_{Guid.NewGuid():N}"[..15], "update")
            );
        });

        Client.AuthenticateAsSuperAdmin();
        AdminBulkUpdateRolePermissionsRequest request = new AdminBulkUpdateRolePermissionsRequestBuilder()
            .WithPermissionIds([permissionId1, permissionId2, permissionId3])
            .Build();

        var response = await Client.PutAsJsonAsync(Routes.Admin.Roles.Permissions(roleId), request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminBulkUpdateRolePermissionsResponse>();
        body.Role.Id.Should().Be(roleId);
        body.Role.Permissions.Select(p => p.Id).Should().BeEquivalentTo([permissionId1, permissionId2, permissionId3]);

        await using IdentityDbContext verifyContext = CreateDbContext<IdentityDbContext>();
        List<RolePermissionEntity> assignments = await verifyContext
            .RolePermissions.Where(rp => rp.RoleId == roleId)
            .ToListAsync();
        assignments.Should().HaveCount(3);
        assignments.Select(a => a.PermissionId).Should().BeEquivalentTo([permissionId1, permissionId2, permissionId3]);
    }

    [Fact]
    public async Task BulkUpdatePermissions_AsVisitor_ReturnsForbidden()
    {
        Guid roleId = Guid.NewGuid();

        await SeedAsync<IdentityDbContext>(ctx =>
        {
            ctx.Roles.Add(RoleFactory.CreateWithId(roleId, $"rpa_{Guid.NewGuid():N}"[..20]));
        });

        Client.AuthenticateAsVisitor();
        AdminBulkUpdateRolePermissionsRequest request = new AdminBulkUpdateRolePermissionsRequestBuilder().Build();

        var response = await Client.PutAsJsonAsync(Routes.Admin.Roles.Permissions(roleId), request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
