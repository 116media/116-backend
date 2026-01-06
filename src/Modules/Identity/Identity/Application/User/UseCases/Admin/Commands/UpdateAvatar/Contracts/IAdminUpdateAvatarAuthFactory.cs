using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Domain.Entities;

namespace _116.Identity.Application.User.UseCases.Admin.Commands.UpdateAvatar.Contracts;

/// <summary>
/// Contains updated admin user data with avatar and associated roles/permissions.
/// </summary>
public record AdminUpdateAvatarAuthData(
    UserEntity User,
    IReadOnlyCollection<RoleDto> Roles,
    IReadOnlyCollection<PermissionDto> Permissions
);

/// <summary>
/// Factory for handling admin user avatar update logic.
/// </summary>
public interface IAdminUpdateAvatarAuthFactory
{
    /// <summary>
    /// Gets and validates admin user for avatar update.
    /// </summary>
    /// <param name="userId">The ID of the admin user.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Admin user data for avatar update.</returns>
    Task<AdminUpdateAvatarAuthData> GetUserForAvatarUpdateAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Updates an admin user's avatar with a new image file.
    /// </summary>
    /// <param name="user">The admin user entity to update.</param>
    /// <param name="avatarFileId">The ID of the new avatar file.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Update data containing user, roles, and permissions.</returns>
    Task<AdminUpdateAvatarAuthData> UpdateAvatarAsync(
        UserEntity user,
        Guid avatarFileId,
        CancellationToken cancellationToken
    );
}
