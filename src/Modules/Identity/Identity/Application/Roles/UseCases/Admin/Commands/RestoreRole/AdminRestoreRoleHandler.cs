using _116.Identity.Application.Shared.Errors;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.RestoreRole;

/// <summary>
/// Handles the <see cref="AdminRestoreRoleCommand" /> to restore a soft-deleted role.
/// </summary>
/// <param name="roleRepository">Repository for role data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminRestoreRoleHandler(IRoleRepository roleRepository, IIdentityUnitOfWork unitOfWork)
    : ICommandHandler<AdminRestoreRoleCommand, AdminRestoreRoleResult>
{
    /// <summary>
    /// Handles the role restore command.
    /// </summary>
    /// <param name="command">The command containing the role ID to restore.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="AdminRestoreRoleResult" /> containing the restored role.</returns>
    public async Task<AdminRestoreRoleResult> Handle(
        AdminRestoreRoleCommand command,
        CancellationToken cancellationToken
    )
    {
        RoleEntity? role = await roleRepository.GetRoleByIdOrThrowAsync(
            roleId: command.RoleId,
            cancellationToken: cancellationToken
        );

        bool wasRestored = role!.Restore();

        if (!wasRestored)
        {
            throw UserErrors.RoleNotDeleted();
        }

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        var roleDto = role.ToRoleDto();
        return new AdminRestoreRoleResult(Role: roleDto);
    }
}
