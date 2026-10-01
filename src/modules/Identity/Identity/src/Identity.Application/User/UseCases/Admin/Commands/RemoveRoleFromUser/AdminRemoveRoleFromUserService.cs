using _116.Identity.Application.Shared.Errors.Facade;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Application.User.UseCases.Admin.Commands.RemoveRoleFromUser.Contracts;
using _116.Identity.Domain.Entities;

namespace _116.Identity.Application.User.UseCases.Admin.Commands.RemoveRoleFromUser;

/// <summary>
/// Resolves and applies a role revocation for the admin remove-role use case.
/// </summary>
/// <param name="authRepository">Repository loading the user aggregate with its roles.</param>
/// <param name="roleRepository">Repository resolving the revoked role.</param>
/// <param name="i18n">Single i18n entry point for the Identity module.</param>
public class AdminRemoveRoleFromUserService(
    IAuthRepository authRepository,
    IRoleRepository roleRepository,
    IdentityI18n i18n
) : IAdminRemoveRoleFromUserService
{
    /// <inheritdoc />
    public async Task<UserEntity> RevokeAsync(Guid userId, Guid roleId, CancellationToken cancellationToken)
    {
        // The role name rides the revocation event.
        RoleEntity? removedRole = await roleRepository.GetRoleByIdOrThrowAsync(
            roleId: roleId,
            cancellationToken: cancellationToken
        );

        UserEntity? user = await authRepository.GetUserWithRolesByIdOrThrow(
            userId: userId,
            cancellationToken: cancellationToken
        );

        if (!user!.RevokeRole(roleId: roleId, roleName: removedRole!.Name))
        {
            throw i18n.User.RoleNotAssignedToUser();
        }

        return user;
    }
}
