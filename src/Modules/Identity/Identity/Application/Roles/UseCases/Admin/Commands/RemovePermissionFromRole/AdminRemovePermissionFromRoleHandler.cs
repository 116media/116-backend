using _116.Identity.Application.Shared.Errors;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.RemovePermissionFromRole;

/// <summary>
/// Handles the <see cref="AdminRemovePermissionFromRoleCommand" /> to remove a permission from a role.
/// </summary>
/// <param name="roleRepository">Repository for role data access operations.</param>
/// <param name="rolePermissionRepository">Repository for role-permission data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminRemovePermissionFromRoleHandler(
    IRoleRepository roleRepository,
    IRolePermissionRepository rolePermissionRepository,
    IIdentityUnitOfWork unitOfWork,
    IMapper mapper
) : ICommandHandler<AdminRemovePermissionFromRoleCommand, AdminRemovePermissionFromRoleResult>
{
    /// <summary>
    /// Handles the remove permission from role command.
    /// </summary>
    /// <param name="command">The command containing the role ID and permission ID.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="AdminRemovePermissionFromRoleResult" /> containing the updated role.</returns>
    public async Task<AdminRemovePermissionFromRoleResult> Handle(
        AdminRemovePermissionFromRoleCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid roleId = Guid.Parse(input: command.RoleId);
        Guid permissionId = Guid.Parse(input: command.PermissionId);

        // Validate role exists
        await roleRepository.GetRoleByIdOrThrowAsync(roleId: roleId, cancellationToken: cancellationToken);

        // Get the role-permission association
        RolePermissionEntity? rolePermission = await rolePermissionRepository.GetByRoleAndPermissionAsync(
            roleId: roleId,
            permissionId: permissionId,
            cancellationToken: cancellationToken
        );

        if (rolePermission is null)
        {
            throw UserErrors.PermissionNotAssignedToRole();
        }

        // Remove the association
        rolePermissionRepository.Delete(entity: rolePermission);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        // Reload role with permissions to return updated data
        RoleEntity? role = await roleRepository.GetRoleByIdWithPermissionsOrThrowAsync(
            roleId: roleId,
            cancellationToken: cancellationToken
        );

        var roleDto = role!.ToRoleWithPermissionsDto(mapper);
        return new AdminRemovePermissionFromRoleResult(Role: roleDto, IsSuccess: true);
    }
}
