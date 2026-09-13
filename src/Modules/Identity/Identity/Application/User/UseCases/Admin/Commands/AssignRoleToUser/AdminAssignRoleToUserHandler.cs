using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.Shared.Errors.Facade;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Identity.Application.User.UseCases.Admin.Commands.AssignRoleToUser;

/// <summary>
/// Handles the <see cref="AdminAssignRoleToUserCommand" /> to grant a role through the user
/// aggregate, bumping the target user's token version so outstanding tokens pick up the grant
/// on refresh.
/// </summary>
/// <param name="roleRepository">Repository for role data access operations.</param>
/// <param name="authRepository">Repository loading the user aggregate with its roles.</param>
/// <param name="tokenStateRepository">Repository bumping the target user's token version.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
/// <param name="i18n">Single i18n entry point for the Identity module.</param>
public class AdminAssignRoleToUserHandler(
    IRoleRepository roleRepository,
    IAuthRepository authRepository,
    IUserTokenStateRepository tokenStateRepository,
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

        // Soft deletion also clears IsActive, so the deleted check comes first to stay reachable.
        if (role!.IsDeleted)
        {
            throw i18n.User.RoleIsDeleted();
        }

        if (!role.IsActive)
        {
            throw i18n.User.RoleIsInactive();
        }

        UserEntity? user = await authRepository.GetUserWithRolesByIdOrThrow(
            userId: userId,
            cancellationToken: cancellationToken
        );

        if (!user!.GrantRole(roleId: command.RoleId, roleName: role.Name))
        {
            throw i18n.User.RoleAlreadyAssignedToUser();
        }

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        await tokenStateRepository.BumpTokenVersionAsync(userId: userId, cancellationToken: cancellationToken);

        // The freshly granted association has no Role navigation loaded yet; the role in hand fills it.
        IReadOnlyCollection<RoleDto> roles = user
            .UserRoles.Select(ur => (ur.RoleId == role.Id ? role : ur.Role).ToRoleDto(mapper))
            .ToList();
        return new AdminAssignRoleToUserResult(Roles: roles);
    }
}
