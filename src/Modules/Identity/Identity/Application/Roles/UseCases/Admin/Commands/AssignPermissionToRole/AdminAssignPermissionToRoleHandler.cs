using _116.Identity.Application.Shared.Errors.Facade;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.AssignPermissionToRole;

/// <summary>
/// Handles the <see cref="AdminAssignPermissionToRoleCommand" /> to grant a permission through
/// the role aggregate, bumping every role member's token version.
/// </summary>
/// <param name="roleRepository">Repository for role data access operations.</param>
/// <param name="permissionRepository">Repository for permission data access operations.</param>
/// <param name="tokenStateRepository">Repository bumping the role members' token versions.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
/// <param name="i18n">Single i18n entry point for the Identity module.</param>
public class AdminAssignPermissionToRoleHandler(
    IRoleRepository roleRepository,
    IPermissionRepository permissionRepository,
    IUserTokenStateRepository tokenStateRepository,
    IIdentityUnitOfWork unitOfWork,
    IMapper mapper,
    IdentityI18n i18n
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

        // Load the role with its permissions so the grant goes through the aggregate
        RoleEntity? role = await roleRepository.GetRoleByIdWithPermissionsOrThrowAsync(
            roleId: roleId,
            cancellationToken: cancellationToken
        );

        // Soft deletion also clears IsActive, so the deleted state is checked first to keep the
        // more specific error reachable.
        if (role!.IsDeleted)
        {
            throw i18n.User.RoleIsDeleted();
        }

        if (!role.IsActive)
        {
            throw i18n.User.RoleIsInactive();
        }

        // Validate permission exists
        PermissionEntity? permission = await permissionRepository.GetPermissionByIdOrThrowAsync(
            permissionId: command.PermissionId,
            cancellationToken: cancellationToken
        );

        // Soft deletion also clears IsActive, so the deleted state is checked first to keep the
        // more specific error reachable.
        if (permission!.IsDeleted)
        {
            throw i18n.User.PermissionIsDeleted();
        }

        if (!permission.IsActive)
        {
            throw i18n.User.PermissionIsInactive();
        }

        if (!role.GrantPermission(permissionId: command.PermissionId))
        {
            throw i18n.User.PermissionAlreadyAssignedToRole();
        }

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        await tokenStateRepository.BumpTokenVersionForRoleUsersAsync(
            roleId: roleId,
            cancellationToken: cancellationToken
        );

        // Reload the role with permissions to return updated data
        role = await roleRepository.GetRoleByIdWithPermissionsOrThrowAsync(
            roleId: roleId,
            cancellationToken: cancellationToken
        );

        var roleDto = role!.ToRoleWithPermissionsDto(mapper);
        return new AdminAssignPermissionToRoleResult(Role: roleDto);
    }
}
