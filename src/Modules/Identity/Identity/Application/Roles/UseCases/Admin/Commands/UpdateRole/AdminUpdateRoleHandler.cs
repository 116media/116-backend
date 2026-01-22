using _116.Identity.Application.Shared.Errors;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.UpdateRole;

/// <summary>
/// Handles the <see cref="AdminUpdateRoleCommand" /> to update an existing role.
/// </summary>
/// <param name="roleRepository">Repository for role data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminUpdateRoleHandler(IRoleRepository roleRepository, IIdentityUnitOfWork unitOfWork)
    : ICommandHandler<AdminUpdateRoleCommand, AdminUpdateRoleResult>
{
    /// <summary>
    /// Handles the role update command.
    /// </summary>
    /// <param name="command">The command containing role ID and update data.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="AdminUpdateRoleResult" /> containing the updated role.</returns>
    public async Task<AdminUpdateRoleResult> Handle(AdminUpdateRoleCommand command, CancellationToken cancellationToken)
    {
        RoleEntity? role = await roleRepository.GetRoleByIdOrThrowAsync(
            roleId: command.RoleId,
            cancellationToken: cancellationToken
        );

        string? name = command.Name;
        string? description = command.Description;

        bool isNameUpdated = !string.IsNullOrEmpty(value: name) && role!.Name != name;
        bool isDescriptionUpdated = !string.IsNullOrEmpty(value: description) && role!.Description != description;

        if (isNameUpdated)
        {
            await EnsureRoleNameUnique(name: name!, cancellationToken: cancellationToken);
        }

        if (isNameUpdated || isDescriptionUpdated)
        {
            string newName = isNameUpdated ? name! : role!.Name;
            string newDescription = isDescriptionUpdated ? description! : role!.Description;

            role!.Update(name: newName, description: newDescription);
            await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
        }

        var roleDto = role!.ToRoleDto();
        return new AdminUpdateRoleResult(Role: roleDto);
    }

    private async Task EnsureRoleNameUnique(string name, CancellationToken cancellationToken)
    {
        if (await roleRepository.ExistsByNameAsync(name: name, cancellationToken: cancellationToken))
        {
            throw UserErrors.RoleAlreadyExists(roleName: name);
        }
    }
}
