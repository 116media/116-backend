using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Application.User.UseCases.Public.Commands.UpdateAvatar.Contracts;
using _116.Identity.Domain.Entities;

namespace _116.Identity.Application.User.UseCases.Public.Commands.UpdateAvatar;

/// <summary>
/// Factory implementation for handling user avatar update logic.
/// </summary>
/// <param name="authRepository">Repository for user data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class PublicUpdateAvatarAuthFactory(IAuthRepository authRepository, IIdentityUnitOfWork unitOfWork)
    : IPublicUpdateAvatarAuthFactory
{
    /// <summary>
    /// Gets and validates user for avatar update.
    /// </summary>
    public async Task<PublicUpdateAvatarAuthData> GetUserForAvatarUpdateAsync(
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
        authRepository.IsUserAccountVerified(user!);
        await authRepository.IsSessionValidAsync(sessionId, cancellationToken);

        return new PublicUpdateAvatarAuthData(User: user!);
    }

    /// <summary>
    /// Updates a user's avatar with a new image file.
    /// </summary>
    public async Task<PublicUpdateAvatarAuthData> UpdateAvatarAsync(
        UserEntity user,
        Guid avatarFileId,
        CancellationToken cancellationToken
    )
    {
        user.UpdateAvatar(avatarFileId: avatarFileId, avatarSource: Domain.Enums.EnumAvatarSource.Manual);

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new PublicUpdateAvatarAuthData(User: user);
    }
}
