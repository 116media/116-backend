using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.Shared.Errors;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Identity.Application.User.UseCases.Admin.Commands.RemoveRoleFromUser;

/// <summary>
/// Handles the <see cref="AdminRemoveRoleFromUserCommand" /> to remove a role from a user.
/// </summary>
/// <param name="userRoleRepository">Repository for user-role data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminRemoveRoleFromUserHandler(
    IUserRoleRepository userRoleRepository,
    IIdentityUnitOfWork unitOfWork,
    IMapper mapper
) : ICommandHandler<AdminRemoveRoleFromUserCommand, AdminRemoveRoleFromUserResult>
{
    /// <summary>
    /// Handles the remove role from user command.
    /// </summary>
    /// <param name="command">The command containing the user ID and role ID.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="AdminRemoveRoleFromUserResult" /> containing the user's updated roles.</returns>
    public async Task<AdminRemoveRoleFromUserResult> Handle(
        AdminRemoveRoleFromUserCommand command,
        CancellationToken cancellationToken
    )
    {
        // Get the user-role association
        UserRoleEntity? userRole = await userRoleRepository.GetByUserAndRoleAsync(
            userId: command.UserId,
            roleId: command.RoleId,
            cancellationToken: cancellationToken
        );

        if (userRole is null)
        {
            throw UserErrors.RoleNotAssignedToUser();
        }

        // Remove the association
        userRoleRepository.Delete(entity: userRole);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        // Get updated user roles
        List<UserRoleEntity> userRoles = await userRoleRepository.GetUserRolesWithRoleAsync(
            userId: command.UserId,
            cancellationToken: cancellationToken
        );

        IReadOnlyCollection<RoleDto> roles = userRoles.Select(ur => ur.Role.ToRoleDto(mapper)).ToList();
        return new AdminRemoveRoleFromUserResult(Roles: roles, IsSuccess: true);
    }
}
