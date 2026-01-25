using _116.Identity.Application.Shared.Errors;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.ActivateRole;

/// <summary>
/// Handles the <see cref="AdminActivateRoleCommand" /> to activate a role.
/// </summary>
/// <param name="roleRepository">Repository for role data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminActivateRoleHandler(IRoleRepository roleRepository, IIdentityUnitOfWork unitOfWork)
    : ICommandHandler<AdminActivateRoleCommand, AdminActivateRoleResult>
{
    /// <summary>
    /// Handles the role activation command.
    /// </summary>
    /// <param name="command">The command containing the role ID to activate.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="AdminActivateRoleResult" /> containing the activated role.</returns>
    public async Task<AdminActivateRoleResult> Handle(
        AdminActivateRoleCommand command,
        CancellationToken cancellationToken
    )
    {
        RoleEntity? role = await roleRepository.GetRoleByIdOrThrowAsync(
            roleId: command.RoleId,
            cancellationToken: cancellationToken
        );

        bool wasActivated = role!.Activate();

        if (!wasActivated)
        {
            throw UserErrors.RoleAlreadyActive();
        }

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        var roleDto = role.ToRoleDto();
        return new AdminActivateRoleResult(Role: roleDto);
    }
}
