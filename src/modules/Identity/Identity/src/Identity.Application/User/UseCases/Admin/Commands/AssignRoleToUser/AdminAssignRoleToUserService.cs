using _116.Identity.Application.Shared.Errors.Facade;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Application.User.UseCases.Admin.Commands.AssignRoleToUser.Contracts;
using _116.Identity.Domain.Entities;

namespace _116.Identity.Application.User.UseCases.Admin.Commands.AssignRoleToUser;

/// <summary>
/// Resolves and applies a role grant for the admin assign-role use case.
/// </summary>
/// <param name="roleRepository">Repository for role data access operations.</param>
/// <param name="authRepository">Repository loading the user aggregate with its roles.</param>
/// <param name="i18n">Single i18n entry point for the Identity module.</param>
public class AdminAssignRoleToUserService(
    IRoleRepository roleRepository,
    IAuthRepository authRepository,
    IdentityI18n i18n
) : IAdminAssignRoleToUserService
{
    /// <inheritdoc />
    public async Task<RoleGrantData> GrantAsync(Guid userId, Guid roleId, CancellationToken cancellationToken)
    {
        RoleEntity? role = await roleRepository.GetRoleByIdOrThrowAsync(
            roleId: roleId,
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

        if (!user!.GrantRole(roleId: roleId, roleName: role.Name))
        {
            throw i18n.User.RoleAlreadyAssignedToUser();
        }

        return new RoleGrantData(User: user, Role: role);
    }
}
