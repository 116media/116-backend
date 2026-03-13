using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.Shared.Errors;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Identity.Application.User.UseCases.Admin.Commands.AssignRoleToUser;

/// <summary>
/// Handles the <see cref="AdminAssignRoleToUserCommand" /> to assign a role to a user.
/// </summary>
/// <param name="roleRepository">Repository for role data access operations.</param>
/// <param name="userRoleRepository">Repository for user-role data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminAssignRoleToUserHandler(
    IRoleRepository roleRepository,
    IUserRoleRepository userRoleRepository,
    IIdentityUnitOfWork unitOfWork,
    IMapper mapper
) : ICommandHandler<AdminAssignRoleToUserCommand, AdminAssignRoleToUserResult>
{
    /// <summary>
    /// Handles the assign role to user command.
    /// </summary>
    /// <param name="command">The command containing the user ID and role ID.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="AdminAssignRoleToUserResult" /> containing the user's updated roles.</returns>
    public async Task<AdminAssignRoleToUserResult> Handle(
        AdminAssignRoleToUserCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid userId = Guid.Parse(input: command.UserId);

        // Validate role exists
        RoleEntity? role = await roleRepository.GetRoleByIdOrThrowAsync(
            roleId: command.RoleId,
            cancellationToken: cancellationToken
        );

        // Check if role is active
        if (!role!.IsActive)
        {
            throw UserErrors.RoleIsInactive();
        }

        // Check if role is deleted
        if (role.IsDeleted)
        {
            throw UserErrors.RoleIsDeleted();
        }

        // Check if role is already assigned to user
        bool alreadyAssigned = await userRoleRepository.ExistsByUserAndRoleAsync(
            userId: userId,
            roleId: command.RoleId,
            cancellationToken: cancellationToken
        );

        if (alreadyAssigned)
        {
            throw UserErrors.RoleAlreadyAssignedToUser();
        }

        // Create the user-role association
        var userRole = UserRoleEntity.Create(id: Guid.NewGuid(), userId: userId, roleId: command.RoleId);

        await userRoleRepository.AddAsync(entity: userRole, cancellationToken: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        // Get updated user roles
        List<UserRoleEntity> userRoles = await userRoleRepository.GetUserRolesWithRoleAsync(
            userId: userId,
            cancellationToken: cancellationToken
        );

        IReadOnlyCollection<RoleDto> roles = userRoles.Select(ur => ur.Role.ToRoleDto(mapper)).ToList();
        return new AdminAssignRoleToUserResult(Roles: roles);
    }
}
