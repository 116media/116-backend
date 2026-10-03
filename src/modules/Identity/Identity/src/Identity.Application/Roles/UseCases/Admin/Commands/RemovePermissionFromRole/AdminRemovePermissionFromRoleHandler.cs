using _116.BuildingBlocks.Application.CQRS;
using _116.Identity.Application.Roles.UseCases.Admin.Commands.RemovePermissionFromRole.Contracts;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using MapsterMapper;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.RemovePermissionFromRole;

/// <summary>
/// Handles the <see cref="AdminRemovePermissionFromRoleCommand" /> to revoke a permission through
/// the role aggregate, bumping the token version of every user holding the role.
/// </summary>
/// <param name="removePermissionService">Service resolving and applying the revocation.</param>
/// <param name="tokenStateRepository">Repository bumping the role holders' token versions.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminRemovePermissionFromRoleHandler(
    IAdminRemovePermissionFromRoleService removePermissionService,
    IUserTokenStateRepository tokenStateRepository,
    IIdentityUnitOfWork unitOfWork,
    IMapper mapper
) : ICommandHandler<AdminRemovePermissionFromRoleCommand, AdminRemovePermissionFromRoleResult>
{
    /// <inheritdoc />
    public async Task<AdminRemovePermissionFromRoleResult> Handle(
        AdminRemovePermissionFromRoleCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid roleId = Guid.Parse(input: command.RoleId);
        Guid permissionId = Guid.Parse(input: command.PermissionId);

        RoleEntity role = await removePermissionService.RevokeAsync(
            roleId: roleId,
            permissionId: permissionId,
            cancellationToken: cancellationToken
        );

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
        await tokenStateRepository.BumpTokenVersionForRoleUsersAsync(
            roleId: roleId,
            cancellationToken: cancellationToken
        );

        var roleDto = role.ToRoleWithPermissionsDto(mapper);
        return new AdminRemovePermissionFromRoleResult(Role: roleDto, IsSuccess: true);
    }
}
