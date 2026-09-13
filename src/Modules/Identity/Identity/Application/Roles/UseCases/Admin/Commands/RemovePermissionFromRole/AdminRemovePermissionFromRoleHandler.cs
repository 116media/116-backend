using _116.Identity.Application.Shared.Errors.Facade;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.RemovePermissionFromRole;

/// <summary>
/// Handles the <see cref="AdminRemovePermissionFromRoleCommand" /> to revoke a permission
/// through the role aggregate, bumping every role member's token version.
/// </summary>
/// <param name="roleRepository">Repository for role data access operations.</param>
/// <param name="tokenStateRepository">Repository bumping the role members' token versions.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
/// <param name="i18n">Single i18n entry point for the Identity module.</param>
public class AdminRemovePermissionFromRoleHandler(
    IRoleRepository roleRepository,
    IUserTokenStateRepository tokenStateRepository,
    IIdentityUnitOfWork unitOfWork,
    IMapper mapper,
    IdentityI18n i18n
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

        // Load the role with its permissions so the revocation goes through the aggregate
        RoleEntity? role = await roleRepository.GetRoleByIdWithPermissionsOrThrowAsync(
            roleId: roleId,
            cancellationToken: cancellationToken
        );

        if (!role!.RevokePermission(permissionId: permissionId))
        {
            throw i18n.User.PermissionNotAssignedToRole();
        }

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        await tokenStateRepository.BumpTokenVersionForRoleUsersAsync(
            roleId: roleId,
            cancellationToken: cancellationToken
        );

        var roleDto = role.ToRoleWithPermissionsDto(mapper);
        return new AdminRemovePermissionFromRoleResult(Role: roleDto, IsSuccess: true);
    }
}
