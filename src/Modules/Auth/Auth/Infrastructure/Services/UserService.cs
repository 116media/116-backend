using _116.Core.Application.Shared.Services;
using _116.Shared.Application.Exceptions;
using _116.Auth.Application.Shared.Errors;
using _116.Auth.Application.Shared.Repositories;
using _116.Auth.Application.Shared.Services;
using _116.Auth.Domain.Entities;
using _116.Auth.Domain.Enums;

namespace _116.Auth.Infrastructure.Services;

/// <summary>
/// Implementation of user service for user management operations.
/// </summary>
public class UserService(IUserRepository userRepository, IFileService fileService) : IUserService
{
    /// <summary>
    /// Gets the existing external user or creates a new one for social authentication.
    /// </summary>
    /// <param name="email">User's email address.</param>
    /// <param name="userName">Username from social provider.</param>
    /// <param name="authProvider">Authentication provider.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>User entity with roles and permissions loaded.</returns>
    /// <exception cref="ConflictException">Thrown when a local account exists with the same email.</exception>
    public async Task<UserEntity> GetOrCreateExternalUserAsync(
        string email,
        string? userName,
        AuthProvider authProvider,
        CancellationToken cancellationToken
    )
    {
        UserEntity user;

        try
        {
            // Try to load existing user including roles and permissions
            user = await userRepository.GetPublicUserWithRolesAndPermissionsAsync(
                email, cancellationToken
            );

            // Prevent social login if a local account exists
            if (user.AuthProvider == AuthProvider.Local)
            {
                throw UserErrors.EmailAlreadyExists(email);
            }

            // Update username if a new one is provided and it's different
            if (!string.IsNullOrWhiteSpace(userName) && user.UserName != userName)
            {
                // Check if another user already takes the new username
                bool usernameExists = await userRepository.ExistsByUserNameAsync(userName, cancellationToken);
                if (usernameExists)
                {
                    throw UserErrors.UsernameAlreadyExists(userName);
                }

                user.UpdateUserName(userName);
            }
        }
        catch (NotFoundException)
        {
            // User doesn't exist, create a new one
            user = UserEntity.CreateExternal(
                id: Guid.NewGuid(),
                userName: userName!,
                authProvider: authProvider,
                email: email
            );

            await userRepository.AddAsync(user, cancellationToken);
            await userRepository.AssignVisitorRoleAsync(user.Id, cancellationToken);
            await userRepository.SaveChangesAsync(cancellationToken);

            // Reload user with roles and permissions after creation
            user = await userRepository.GetPublicUserWithRolesAndPermissionsAsync(
                email, cancellationToken
            );
        }

        return user;
    }

    /// <summary>
    /// Updates user's avatar from external URL if needed.
    /// </summary>
    /// <param name="user">User entity to update.</param>
    /// <param name="avatarUrl">Avatar URL from external provider.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated user entity.</returns>
    public async Task<UserEntity> UpdateUserAvatarAsync(
        UserEntity user,
        string? avatarUrl,
        CancellationToken cancellationToken
    )
    {
        // Skip if no avatar URL provided
        if (string.IsNullOrEmpty(avatarUrl))
        {
            return user;
        }

        // Download and update avatar
        Guid avatarFileId = await fileService.GetOrDownloadFileAsync(
            user.AvatarFileId,
            avatarUrl,
            cancellationToken
        );
        user.UpdateAvatar(avatarFileId);

        return user;
    }
}
