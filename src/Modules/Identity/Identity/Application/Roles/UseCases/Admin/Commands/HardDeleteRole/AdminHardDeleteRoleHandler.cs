using _116.Identity.Application.Shared.Errors;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.HardDeleteRole;

/// <summary>
/// Handles the <see cref="AdminHardDeleteRoleCommand" /> to permanently delete a role.
/// </summary>
/// <param name="roleRepository">Repository for role data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminHardDeleteRoleHandler(IRoleRepository roleRepository, IIdentityUnitOfWork unitOfWork)
    : ICommandHandler<AdminHardDeleteRoleCommand, AdminHardDeleteRoleResult>
{
    /// <summary>
    /// Handles the role hard delete command.
    /// </summary>
    /// <param name="command">The command containing the role ID to permanently delete.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="AdminHardDeleteRoleResult" /> indicating success.</returns>
    public async Task<AdminHardDeleteRoleResult> Handle(
        AdminHardDeleteRoleCommand command,
        CancellationToken cancellationToken
    )
    {
        RoleEntity? role = await roleRepository.GetRoleByIdOrThrowAsync(
            roleId: command.RoleId,
            cancellationToken: cancellationToken
        );

        // Protect core roles from being deleted
        if (IsCoreRole(roleName: role!.Name))
        {
            throw UserErrors.CoreRoleCannotBeDeleted(roleName: role.Name);
        }

        roleRepository.Delete(entity: role);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminHardDeleteRoleResult(Success: true);
    }

    private static bool IsCoreRole(string roleName)
    {
        return roleName == nameof(EnumCoreUserRole.SuperAdmin)
            || roleName == nameof(EnumCoreUserRole.Admin)
            || roleName == nameof(EnumCoreUserRole.Visitor);
    }
}
