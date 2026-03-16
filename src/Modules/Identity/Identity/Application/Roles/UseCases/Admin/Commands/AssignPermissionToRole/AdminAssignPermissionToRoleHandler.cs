using _116.Identity.Application.Shared.Errors;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.AssignPermissionToRole;

/// <summary>
/// Handles the <see cref="AdminAssignPermissionToRoleCommand" /> to assign a permission to a role.
/// </summary>
/// <param name="roleRepository">Repository for role data access operations.</param>
/// <param name="permissionRepository">Repository for permission data access operations.</param>
/// <param name="rolePermissionRepository">Repository for role-permission data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminAssignPermissionToRoleHandler(
    IRoleRepository roleRepository,
    IPermissionRepository permissionRepository,
    IRolePermissionRepository rolePermissionRepository,
    IIdentityUnitOfWork unitOfWork,
    IMapper mapper
) : ICommandHandler<AdminAssignPermissionToRoleCommand, AdminAssignPermissionToRoleResult>
{
    /// <summary>
    /// Handles the assign permission to role command.
    /// </summary>
    /// <param name="command">The command containing the role ID and permission ID.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="AdminAssignPermissionToRoleResult" /> containing the updated role.</returns>
    public async Task<AdminAssignPermissionToRoleResult> Handle(
        AdminAssignPermissionToRoleCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid roleId = Guid.Parse(input: command.RoleId);

        // Validate role exists
        RoleEntity? role = await roleRepository.GetRoleByIdOrThrowAsync(
            roleId: roleId,
            cancellationToken: cancellationToken
        );

        // Check if the role is active and not deleted
        if (!role!.IsActive)
        {
            throw UserErrors.RoleIsInactive();
        }

        if (role.IsDeleted)
        {
            throw UserErrors.RoleIsDeleted();
        }

        // Validate permission exists
        PermissionEntity? permission = await permissionRepository.GetPermissionByIdOrThrowAsync(
            permissionId: command.PermissionId,
            cancellationToken: cancellationToken
        );

        // Check if permission is active and not deleted
        if (!permission!.IsActive)
        {
            throw UserErrors.PermissionIsInactive();
        }

        if (permission.IsDeleted)
        {
            throw UserErrors.PermissionIsDeleted();
        }

        // Check if permission is already assigned to the role
        bool alreadyAssigned = await rolePermissionRepository.ExistsByRoleAndPermissionAsync(
            roleId: roleId,
            permissionId: command.PermissionId,
            cancellationToken: cancellationToken
        );

        if (alreadyAssigned)
        {
            throw UserErrors.PermissionAlreadyAssignedToRole();
        }

        // Create the role-permission association
        var rolePermission = RolePermissionEntity.Create(
            id: Guid.NewGuid(),
            roleId: roleId,
            permissionId: command.PermissionId
        );

        await rolePermissionRepository.AddAsync(entity: rolePermission, cancellationToken: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        // Reload the role with permissions to return updated data
        role = await roleRepository.GetRoleByIdWithPermissionsOrThrowAsync(
            roleId: roleId,
            cancellationToken: cancellationToken
        );

        var roleDto = role!.ToRoleWithPermissionsDto(mapper);
        return new AdminAssignPermissionToRoleResult(Role: roleDto);
    }
}
