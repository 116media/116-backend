using _116.Identity.Domain.Entities;

namespace _116.Identity.Application.User.UseCases.Public.Commands.UpdateAvatar.Contracts;

/// <summary>
/// Contains updated user data with avatar.
/// </summary>
public record PublicUpdateAvatarAuthData(UserEntity User);

/// <summary>
/// Factory for handling user avatar update logic.
/// </summary>
public interface IPublicUpdateAvatarAuthFactory
{
    /// <summary>
    /// Gets and validates user for avatar update.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="sessionId">The user session ID</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>User data for avatar update.</returns>
    Task<PublicUpdateAvatarAuthData> GetUserForAvatarUpdateAsync(
        Guid userId,
        Guid sessionId,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Updates a user's avatar with a new image file.
    /// </summary>
    /// <param name="user">The user entity to update.</param>
    /// <param name="avatarFileId">The ID of the new avatar file.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Update data containing user, roles, and permissions.</returns>
    Task<PublicUpdateAvatarAuthData> UpdateAvatarAsync(
        UserEntity user,
        Guid avatarFileId,
        CancellationToken cancellationToken
    );
}
