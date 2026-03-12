using _116.Identity.Application.Shared.Errors;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.DeactivateRole;

/// <summary>
/// Handles the <see cref="AdminDeactivateRoleCommand" /> to deactivate a role.
/// </summary>
/// <param name="roleRepository">Repository for role data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminDeactivateRoleHandler(IRoleRepository roleRepository, IIdentityUnitOfWork unitOfWork, IMapper mapper)
    : ICommandHandler<AdminDeactivateRoleCommand, AdminDeactivateRoleResult>
{
    /// <summary>
    /// Handles the role deactivation command.
    /// </summary>
    /// <param name="command">The command containing the role ID to deactivate.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="AdminDeactivateRoleResult" /> containing the deactivated role.</returns>
    public async Task<AdminDeactivateRoleResult> Handle(
        AdminDeactivateRoleCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid roleId = Guid.Parse(input: command.RoleId);

        RoleEntity? role = await roleRepository.GetRoleByIdOrThrowAsync(
            roleId: roleId,
            cancellationToken: cancellationToken
        );

        bool wasDeactivated = role!.Deactivate();

        if (!wasDeactivated)
        {
            throw UserErrors.RoleAlreadyInactive();
        }

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        var roleDto = role.ToRoleDto(mapper);
        return new AdminDeactivateRoleResult(Role: roleDto);
    }
}
