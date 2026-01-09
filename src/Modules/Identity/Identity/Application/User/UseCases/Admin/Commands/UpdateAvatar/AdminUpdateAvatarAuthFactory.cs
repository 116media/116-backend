using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Application.User.UseCases.Admin.Commands.UpdateAvatar.Contracts;
using _116.Identity.Domain.Entities;

namespace _116.Identity.Application.User.UseCases.Admin.Commands.UpdateAvatar;

/// <summary>
/// Factory implementation for handling admin user avatar update logic.
/// </summary>
/// <param name="authRepository">Repository for user data access operations.</param>
/// <param name="roleRepository">Repository for role and permission data operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminUpdateAvatarAuthFactory(
    IAuthRepository authRepository,
    IRoleRepository roleRepository,
    IIdentityUnitOfWork unitOfWork
) : IAdminUpdateAvatarAuthFactory
{
    /// <summary>
    /// Gets and validates admin user for avatar update.
    /// </summary>
    public async Task<AdminUpdateAvatarAuthData> GetUserForAvatarUpdateAsync(
        Guid userId,
        CancellationToken cancellationToken
    )
    {
        UserEntity? user = await authRepository.GetUserWithRolesPermissionsAndSessionsByIdOrThrow(
            userId: userId,
            cancellationToken: cancellationToken
        );

        authRepository.IsUserAccountActive(user!);
        authRepository.IsUserLoggedIn(user!);

        var (roles, permissions) = roleRepository.GetUserRolesAndPermissions(userRoles: user!.UserRoles);

        return new AdminUpdateAvatarAuthData(User: user, Roles: roles, Permissions: permissions);
    }

    /// <summary>
    /// Updates an admin user's avatar with a new image file.
    /// </summary>
    public async Task<AdminUpdateAvatarAuthData> UpdateAvatarAsync(
        UserEntity user,
        Guid avatarFileId,
        CancellationToken cancellationToken
    )
    {
        user.UpdateAvatar(avatarFileId: avatarFileId, avatarSource: Domain.Enums.EnumAvatarSource.Manual);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        var (roles, permissions) = roleRepository.GetUserRolesAndPermissions(userRoles: user.UserRoles);

        return new AdminUpdateAvatarAuthData(User: user, Roles: roles, Permissions: permissions);
    }
}
