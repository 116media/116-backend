using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Application.User.UseCases.Admin.Commands.UpdateAvatar.Contracts;
using _116.Identity.Domain.Entities;

namespace _116.Identity.Application.User.UseCases.Admin.Commands.UpdateAvatar;

/// <summary>
/// Factory implementation for handling admin user avatar update logic.
/// </summary>
/// <param name="authRepository">Repository for user data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminUpdateAvatarAuthFactory(IAuthRepository authRepository, IIdentityUnitOfWork unitOfWork)
    : IAdminUpdateAvatarAuthFactory
{
    /// <summary>
    /// Gets and validates admin user for avatar update.
    /// </summary>
    public async Task<AdminUpdateAvatarAuthData> GetUserForAvatarUpdateAsync(
        Guid userId,
        Guid sessionId,
        CancellationToken cancellationToken
    )
    {
        UserEntity? user = await authRepository.GetUserWithRolesAndPermissionsByIdOrThrow(
            userId: userId,
            cancellationToken: cancellationToken
        );

        authRepository.IsUserAccountActive(user!);
        await authRepository.IsSessionValidAsync(sessionId, cancellationToken);

        return new AdminUpdateAvatarAuthData(User: user!);
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

        return new AdminUpdateAvatarAuthData(User: user);
    }
}
