using _116.Identity.Application.Session.Repositories;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.Shared.Errors.Facade;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Identity.Application.User.UseCases.Admin.Commands.AssignRoleToUser;

/// <summary>
/// Handles the <see cref="AdminAssignRoleToUserCommand" /> to assign a role to a user.
/// Roles are baked into the access token and never re-checked, so the grant and the revocation of
/// the target user's sessions commit together and the new authorization takes effect on the next
/// sign-in. The security email and in-app notification react to the domain event the association
/// raises for the grant.
/// </summary>
/// <param name="roleRepository">Repository for role data access operations.</param>
/// <param name="userRoleRepository">Repository for user-role data access operations.</param>
/// <param name="sessionRepository">Repository revoking the target user's sessions.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
/// <param name="i18n">Single i18n entry point for the Identity module.</param>
public class AdminAssignRoleToUserHandler(
    IRoleRepository roleRepository,
    IUserRoleRepository userRoleRepository,
    ISessionRepository sessionRepository,
    IIdentityUnitOfWork unitOfWork,
    IMapper mapper,
    IdentityI18n i18n
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

        // Soft deletion also clears IsActive, so the deleted state is checked first to keep the
        // more specific error reachable.
        if (role!.IsDeleted)
        {
            throw i18n.User.RoleIsDeleted();
        }

        if (!role.IsActive)
        {
            throw i18n.User.RoleIsInactive();
        }

        // Check if role is already assigned to user
        bool alreadyAssigned = await userRoleRepository.ExistsByUserAndRoleAsync(
            userId: userId,
            roleId: command.RoleId,
            cancellationToken: cancellationToken
        );

        if (alreadyAssigned)
        {
            throw i18n.User.RoleAlreadyAssignedToUser();
        }

        // Create the user-role association; the role name rides the grant event
        var userRole = UserRoleEntity.Create(
            id: Guid.NewGuid(),
            userId: userId,
            roleId: command.RoleId,
            roleName: role.Name
        );

        await userRoleRepository.AddAsync(entity: userRole, cancellationToken: cancellationToken);

        // An authorization change is administrative: every session of the target user is revoked,
        // closing the window in which a stale token still carries the old role set.
        await sessionRepository.DeleteAllByUserIdAsync(
            userId: userId,
            reason: EnumSessionRevokeReason.SecurityInvalidation,
            exemptSessionId: null,
            cancellationToken: cancellationToken
        );

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
