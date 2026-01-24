using _116.Identity.Application.Shared.Errors;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.SoftDeleteRole;

/// <summary>
/// Handles the <see cref="AdminSoftDeleteRoleCommand" /> to soft delete a role.
/// </summary>
/// <param name="roleRepository">Repository for role data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminSoftDeleteRoleHandler(IRoleRepository roleRepository, IIdentityUnitOfWork unitOfWork)
    : ICommandHandler<AdminSoftDeleteRoleCommand, AdminSoftDeleteRoleResult>
{
    /// <summary>
    /// Handles the role soft delete command.
    /// </summary>
    /// <param name="command">The command containing the role ID to soft delete.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="AdminSoftDeleteRoleResult" /> containing the soft deleted role.</returns>
    public async Task<AdminSoftDeleteRoleResult> Handle(
        AdminSoftDeleteRoleCommand command,
        CancellationToken cancellationToken
    )
    {
        RoleEntity? role = await roleRepository.GetRoleByIdOrThrowAsync(
            roleId: command.RoleId,
            cancellationToken: cancellationToken
        );

        bool wasSoftDeleted = role!.SoftDelete();

        if (!wasSoftDeleted)
        {
            throw UserErrors.RoleAlreadyDeleted();
        }

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        var roleDto = role.ToRoleDto();
        return new AdminSoftDeleteRoleResult(Role: roleDto);
    }
}
