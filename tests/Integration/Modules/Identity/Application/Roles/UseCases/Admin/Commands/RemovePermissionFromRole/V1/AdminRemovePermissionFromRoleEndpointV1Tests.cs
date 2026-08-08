using _116.Identity.Application.Roles.UseCases.Admin.Commands.RemovePermissionFromRole.V1;
using _116.Identity.Application.Shared.Errors.Messages;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.RemovePermissionFromRole.V1;

/// <summary>
/// Integration tests for the AdminRemovePermissionFromRole endpoint.
/// </summary>
[Collection("Database")]
public class AdminRemovePermissionFromRoleEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private async Task<bool> AssignmentExistsAsync(Guid roleId, Guid permissionId)
    {
        await using IdentityDbContext ctx = CreateDbContext<IdentityDbContext>();
        return await ctx.RolePermissions.AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);
    }

    [Fact]
    public async Task RemovePermission_AsSuperAdmin_ExistingAssignment_ReturnsSuccess()
    {
        Guid roleId = Guid.NewGuid();
        Guid permissionId = Guid.NewGuid();

        await SeedAsync<IdentityDbContext>(ctx =>
        {
            ctx.Roles.Add(RoleFactory.CreateWithId(roleId, $"rpa_{Guid.NewGuid():N}"[..20]));
            ctx.Permissions.Add(PermissionFactory.CreateWithId(permissionId, $"rpa_{Guid.NewGuid():N}"[..15], "read"));
            ctx.RolePermissions.Add(RolePermissionEntity.Create(Guid.NewGuid(), roleId, permissionId));
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.DeleteAsync(Routes.Admin.Roles.Permission(roleId, permissionId));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminRemovePermissionFromRoleResponse>();
        body.IsSuccess.Should().BeTrue();
        body.Role.Id.Should().Be(roleId);
        body.Role.Permissions.Should().NotContain(p => p.Id == permissionId);

        (await AssignmentExistsAsync(roleId, permissionId)).Should().BeFalse();
    }

    [Fact]
    public async Task RemovePermission_NonExistentAssignment_ReturnsBadRequest()
    {
        Guid roleId = Guid.NewGuid();
        Guid permissionId = Guid.NewGuid();

        await SeedAsync<IdentityDbContext>(ctx =>
        {
            ctx.Roles.Add(RoleFactory.CreateWithId(roleId, $"rpa_{Guid.NewGuid():N}"[..20]));
            ctx.Permissions.Add(PermissionFactory.CreateWithId(permissionId, $"rpa_{Guid.NewGuid():N}"[..15], "read"));
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.DeleteAsync(Routes.Admin.Roles.Permission(roleId, permissionId));

        await response.ShouldBeProblem<BadRequestException>(
            HttpStatusCode.BadRequest,
            Localized<ValidationErrorMessage>(m => m.PermissionNotAssignedToRole())
        );
    }
}
