using _116.Identity.Application.Roles.UseCases.Admin.Commands.AssignPermissionToRole.Contracts;
using _116.Identity.Application.Shared.Errors.Facade;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.AssignPermissionToRole;

/// <summary>
/// Resolves and applies a permission grant for the admin assign-permission use case.
/// </summary>
/// <param name="roleRepository">Repository loading the role aggregate with its permissions.</param>
/// <param name="permissionRepository">Repository resolving the granted permission.</param>
/// <param name="i18n">Single i18n entry point for the Identity module.</param>
public class AdminAssignPermissionToRoleService(
    IRoleRepository roleRepository,
    IPermissionRepository permissionRepository,
    IdentityI18n i18n
) : IAdminAssignPermissionToRoleService
{
    /// <inheritdoc />
    public async Task<PermissionGrantData> GrantAsync(
        Guid roleId,
        Guid permissionId,
        CancellationToken cancellationToken
    )
    {
        RoleEntity? role = await roleRepository.GetRoleByIdWithPermissionsOrThrowAsync(
            roleId: roleId,
            cancellationToken: cancellationToken
        );

        // Soft deletion also clears IsActive, so the deleted check comes first to stay reachable.
        if (role!.IsDeleted)
        {
            throw i18n.User.RoleIsDeleted();
        }

        if (!role.IsActive)
        {
            throw i18n.User.RoleIsInactive();
        }

        PermissionEntity? permission = await permissionRepository.GetPermissionByIdOrThrowAsync(
            permissionId: permissionId,
            cancellationToken: cancellationToken
        );

        if (permission!.IsDeleted)
        {
            throw i18n.User.PermissionIsDeleted();
        }

        if (!permission.IsActive)
        {
            throw i18n.User.PermissionIsInactive();
        }

        if (!role.GrantPermission(permissionId: permissionId))
        {
            throw i18n.User.PermissionAlreadyAssignedToRole();
        }

        return new PermissionGrantData(Role: role, Permission: permission);
    }
}
