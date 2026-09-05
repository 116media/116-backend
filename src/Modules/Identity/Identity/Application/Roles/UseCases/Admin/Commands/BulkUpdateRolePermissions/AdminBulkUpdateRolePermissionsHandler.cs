using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.BulkUpdateRolePermissions;

/// <summary>
/// Handles the <see cref="AdminBulkUpdateRolePermissionsCommand" /> to reconcile a role's
/// permission set through the role aggregate, bumping every role member's token version.
/// </summary>
/// <param name="roleRepository">Repository for role data access operations.</param>
/// <param name="tokenStateRepository">Repository bumping the role members' token versions.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminBulkUpdateRolePermissionsHandler(
    IRoleRepository roleRepository,
    IUserTokenStateRepository tokenStateRepository,
    IIdentityUnitOfWork unitOfWork,
    IMapper mapper
) : ICommandHandler<AdminBulkUpdateRolePermissionsCommand, AdminBulkUpdateRolePermissionsResult>
{
    /// <summary>
    /// Handles the bulk update role permissions command.
    /// </summary>
    /// <param name="command">The command containing the role ID and permission IDs.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="AdminBulkUpdateRolePermissionsResult" /> containing the updated role.</returns>
    public async Task<AdminBulkUpdateRolePermissionsResult> Handle(
        AdminBulkUpdateRolePermissionsCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid roleId = Guid.Parse(input: command.RoleId);

        RoleEntity? role = await roleRepository.GetRoleByIdWithPermissionsOrThrowAsync(
            roleId: roleId,
            cancellationToken: cancellationToken
        );

        // Determine permissions to add and remove
        HashSet<Guid> newPermissionIds = command.PermissionIds.ToHashSet();
        HashSet<Guid> currentPermissionIds = role!.RolePermissions.Select(rp => rp.PermissionId).ToHashSet();

        List<Guid> permissionsToAdd = newPermissionIds.Except(currentPermissionIds).ToList();
        List<Guid> permissionsToRemove = currentPermissionIds.Except(newPermissionIds).ToList();

        foreach (Guid permissionId in permissionsToRemove)
        {
            role.RevokePermission(permissionId: permissionId);
        }

        foreach (Guid permissionId in permissionsToAdd)
        {
            role.GrantPermission(permissionId: permissionId);
        }

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        // Only bump when the permission set actually changed.
        if (permissionsToAdd.Count > 0 || permissionsToRemove.Count > 0)
        {
            await tokenStateRepository.BumpTokenVersionForRoleUsersAsync(
                roleId: roleId,
                cancellationToken: cancellationToken
            );
        }

        // Reload the role with permissions to return updated data
        role = await roleRepository.GetRoleByIdWithPermissionsOrThrowAsync(
            roleId: roleId,
            cancellationToken: cancellationToken
        );

        var roleDto = role!.ToRoleWithPermissionsDto(mapper);
        return new AdminBulkUpdateRolePermissionsResult(Role: roleDto);
    }
}
