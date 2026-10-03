using _116.Identity.Application.Roles.UseCases.Admin.Commands.RemovePermissionFromRole.Contracts;
using _116.Identity.Application.Shared.Errors.Facade;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.RemovePermissionFromRole;

/// <summary>
/// Resolves and applies a permission revocation for the admin remove-permission use case.
/// </summary>
/// <param name="roleRepository">Repository loading the role aggregate with its permissions.</param>
/// <param name="i18n">Single i18n entry point for the Identity module.</param>
public class AdminRemovePermissionFromRoleService(IRoleRepository roleRepository, IdentityI18n i18n)
    : IAdminRemovePermissionFromRoleService
{
    /// <inheritdoc />
    public async Task<RoleEntity> RevokeAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken)
    {
        RoleEntity? role = await roleRepository.GetRoleByIdWithPermissionsOrThrowAsync(
            roleId: roleId,
            cancellationToken: cancellationToken
        );

        if (!role!.RevokePermission(permissionId: permissionId))
        {
            throw i18n.User.PermissionNotAssignedToRole();
        }

        return role;
    }
}
