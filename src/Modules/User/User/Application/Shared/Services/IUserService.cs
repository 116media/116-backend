using _116.User.Domain.Entities;
using _116.User.Domain.Enums;

namespace _116.User.Application.Shared.Services;

/// <summary>
/// Service for user management operations including social login user handling.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Gets existing external user or creates a new one for social authentication.
    /// </summary>
    /// <param name="email">User's email address.</param>
    /// <param name="userName">Username from social provider.</param>
    /// <param name="authProvider">Authentication provider.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>User entity with roles and permissions loaded.</returns>
    Task<UserEntity> GetOrCreateExternalUserAsync(
        string email,
        string? userName,
        AuthProvider authProvider,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Updates user's avatar from external URL if needed.
    /// </summary>
    /// <param name="user">User entity to update.</param>
    /// <param name="avatarUrl">Avatar URL from external provider.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated user entity.</returns>
    Task<UserEntity> UpdateUserAvatarAsync(UserEntity user, string? avatarUrl, CancellationToken cancellationToken);
}
