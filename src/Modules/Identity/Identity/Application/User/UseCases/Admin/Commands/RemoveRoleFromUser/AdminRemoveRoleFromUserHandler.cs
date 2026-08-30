using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.Shared.Errors.Facade;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Identity.Application.User.UseCases.Admin.Commands.RemoveRoleFromUser;

/// <summary>
/// Handles the <see cref="AdminRemoveRoleFromUserCommand" /> to remove a role from a user,
/// bumping the target user's token version so the removed role cannot keep riding a live token.
/// </summary>
/// <param name="userRoleRepository">Repository for user-role data access operations.</param>
/// <param name="tokenStateRepository">Repository bumping the target user's token version.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
/// <param name="i18n">Single i18n entry point for the Identity module.</param>
/// <param name="roleRepository">Repository resolving the revoked role.</param>
public class AdminRemoveRoleFromUserHandler(
    IUserRoleRepository userRoleRepository,
    IUserTokenStateRepository tokenStateRepository,
    IIdentityUnitOfWork unitOfWork,
    IMapper mapper,
    IdentityI18n i18n,
    IRoleRepository roleRepository
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
        Guid userId = Guid.Parse(input: command.UserId);
        Guid roleId = Guid.Parse(input: command.RoleId);

        // Get the user-role association
        UserRoleEntity? userRole = await userRoleRepository.GetByUserAndRoleAsync(
            userId: userId,
            roleId: roleId,
            cancellationToken: cancellationToken
        );

        if (userRole is not null)
        {
            // Remove the association; the role name rides the revocation event
            RoleEntity? removedRole = await roleRepository.GetRoleByIdOrThrowAsync(
                roleId: roleId,
                cancellationToken: cancellationToken
            );

            userRole.RecordRevocation(roleName: removedRole!.Name);
            userRoleRepository.Delete(entity: userRole);
            await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

            await tokenStateRepository.BumpTokenVersionAsync(userId: userId, cancellationToken: cancellationToken);

            // Get updated user roles
            List<UserRoleEntity> userRoles = await userRoleRepository.GetUserRolesWithRoleAsync(
                userId: userId,
                cancellationToken: cancellationToken
            );

            IReadOnlyCollection<RoleDto> roles = userRoles.Select(ur => ur.Role.ToRoleDto(mapper)).ToList();
            return new AdminRemoveRoleFromUserResult(Roles: roles, IsSuccess: true);
        }

        throw i18n.User.RoleNotAssignedToUser();
    }
}
