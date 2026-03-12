using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.BulkUpdateRolePermissions;

/// <summary>
/// Handles the <see cref="AdminBulkUpdateRolePermissionsCommand" /> to bulk update permissions of a role.
/// </summary>
/// <param name="roleRepository">Repository for role data access operations.</param>
/// <param name="rolePermissionRepository">Repository for role-permission data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminBulkUpdateRolePermissionsHandler(
    IRoleRepository roleRepository,
    IRolePermissionRepository rolePermissionRepository,
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

        // Validate role exists
        await roleRepository.GetRoleByIdWithPermissionsOrThrowAsync(
            roleId: roleId,
            cancellationToken: cancellationToken
        );

        // Get current permission IDs
        List<Guid> currentPermissionIds = await rolePermissionRepository.GetPermissionIdsByRoleIdAsync(
            roleId: roleId,
            cancellationToken: cancellationToken
        );

        // Determine permissions to add and remove
        HashSet<Guid> newPermissionIds = command.PermissionIds.ToHashSet();
        HashSet<Guid> currentPermissionIdsSet = currentPermissionIds.ToHashSet();

        List<Guid> permissionsToAdd = newPermissionIds.Except(currentPermissionIdsSet).ToList();
        List<Guid> permissionsToRemove = currentPermissionIdsSet.Except(newPermissionIds).ToList();

        // Remove permissions
        foreach (Guid permissionId in permissionsToRemove)
        {
            RolePermissionEntity? rolePermission = await rolePermissionRepository.GetByRoleAndPermissionAsync(
                roleId: roleId,
                permissionId: permissionId,
                cancellationToken: cancellationToken
            );

            if (rolePermission is not null)
            {
                rolePermissionRepository.Delete(entity: rolePermission);
            }
        }

        // Add new permissions
        foreach (
            RolePermissionEntity rolePermission in permissionsToAdd.Select(permissionId =>
                RolePermissionEntity.Create(id: Guid.NewGuid(), roleId: roleId, permissionId: permissionId)
            )
        )
        {
            await rolePermissionRepository.AddAsync(entity: rolePermission, cancellationToken: cancellationToken);
        }

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        // Reload the role with permissions to return updated data
        RoleEntity? role = await roleRepository.GetRoleByIdWithPermissionsOrThrowAsync(
            roleId: roleId,
            cancellationToken: cancellationToken
        );

        var roleDto = role!.ToRoleWithPermissionsDto(mapper);
        return new AdminBulkUpdateRolePermissionsResult(Role: roleDto);
    }
}
