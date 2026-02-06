using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.Shared.Errors;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.CreateRole;

/// <summary>
/// Handles the <see cref="AdminCreateRoleCommand" /> to create a new role.
/// </summary>
/// <param name="roleRepository">Repository for role data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminCreateRoleHandler(IRoleRepository roleRepository, IIdentityUnitOfWork unitOfWork, IMapper mapper)
    : ICommandHandler<AdminCreateRoleCommand, AdminCreateRoleResult>
{
    /// <summary>
    /// Handles the role creation command.
    /// </summary>
    /// <param name="command">The command containing role name and description.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="AdminCreateRoleResult" /> containing the created role.</returns>
    public async Task<AdminCreateRoleResult> Handle(AdminCreateRoleCommand command, CancellationToken cancellationToken)
    {
        bool roleExists = await roleRepository.ExistsByNameAsync(
            name: command.Name,
            cancellationToken: cancellationToken
        );

        if (roleExists)
        {
            throw UserErrors.RoleAlreadyExists(roleName: command.Name);
        }

        var role = RoleEntity.Create(id: Guid.NewGuid(), name: command.Name, description: command.Description);

        await roleRepository.AddAsync(role: role, cancellationToken: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        var roleDto = role.ToRoleDto(mapper);

        return new AdminCreateRoleResult(Role: roleDto);
    }
}
