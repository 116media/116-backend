using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Domain.Entities;

namespace _116.Identity.Application.User.UseCases.Public.Commands.UpdateAvatar.Contracts;

/// <summary>
/// Contains updated user data with avatar and associated roles/permissions.
/// </summary>
public record PublicUpdateAvatarAuthData(
    UserEntity User,
    IReadOnlyCollection<RoleDto> Roles,
    IReadOnlyCollection<PermissionDto> Permissions
);

/// <summary>
/// Factory for handling user avatar update logic.
/// </summary>
public interface IPublicUpdateAvatarAuthFactory
{
    /// <summary>
    /// Gets and validates user for avatar update.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>User data for avatar update.</returns>
    Task<PublicUpdateAvatarAuthData> GetUserForAvatarUpdateAsync(Guid userId, CancellationToken cancellationToken);

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
