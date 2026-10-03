using _116.BuildingBlocks.Application.CQRS;
using _116.Identity.Application.Roles.UseCases.Admin.Commands.AssignPermissionToRole.Contracts;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using MapsterMapper;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.AssignPermissionToRole;

/// <summary>
/// Handles the <see cref="AdminAssignPermissionToRoleCommand" /> to grant a permission through
/// the role aggregate, bumping the token version of every user holding the role.
/// </summary>
/// <param name="assignPermissionService">Service resolving and applying the grant.</param>
/// <param name="tokenStateRepository">Repository bumping the role holders' token versions.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminAssignPermissionToRoleHandler(
    IAdminAssignPermissionToRoleService assignPermissionService,
    IUserTokenStateRepository tokenStateRepository,
    IIdentityUnitOfWork unitOfWork,
    IMapper mapper
) : ICommandHandler<AdminAssignPermissionToRoleCommand, AdminAssignPermissionToRoleResult>
{
    /// <inheritdoc />
    public async Task<AdminAssignPermissionToRoleResult> Handle(
        AdminAssignPermissionToRoleCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid roleId = Guid.Parse(input: command.RoleId);

        PermissionGrantData grant = await assignPermissionService.GrantAsync(
            roleId: roleId,
            permissionId: command.PermissionId,
            cancellationToken: cancellationToken
        );

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
        await tokenStateRepository.BumpTokenVersionForRoleUsersAsync(
            roleId: roleId,
            cancellationToken: cancellationToken
        );

        RoleWithPermissionsDto roleDto = grant.Role.ToRoleWithPermissionsDto(mapper, granted: grant.Permission);
        return new AdminAssignPermissionToRoleResult(Role: roleDto);
    }
}
