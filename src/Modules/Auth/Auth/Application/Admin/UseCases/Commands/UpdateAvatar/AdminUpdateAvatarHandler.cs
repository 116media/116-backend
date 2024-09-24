using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using _116.Auth.Application.Shared.Mappers;
using _116.Auth.Application.Shared.Repositories;
using _116.Auth.Application.Shared.Services;
using _116.Auth.Domain.Entities;

namespace _116.Auth.Application.Admin.UseCases.Commands.UpdateAvatar;

/// <summary>
/// Handles the <see cref="AdminUpdateAvatarCommand"/> to update admin user avatar.
/// </summary>
/// <param name="userRepository">Repository for user data access operations.</param>
/// <param name="fileRepository">Repository for file data access operations.</param>
/// <param name="roleRepository">Repository for role and permission data operations.</param>
/// <param name="userService">Service for user operations including avatar updates.</param>
public class AdminUpdateAvatarHandler(
    IUserRepository userRepository,
    IFileRepository fileRepository,
    IRoleRepository roleRepository,
    IUserService userService
) : ICommandHandler<AdminUpdateAvatarCommand, AdminUpdateAvatarResult>
{
    /// <summary>
    /// Handles the avatar update command by updating the admin user's avatar URL.
    /// </summary>
    /// <param name="command">The avatar update command containing the admin user ID and new avatar URL.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The result containing the updated admin user information.</returns>
    public async Task<AdminUpdateAvatarResult> Handle(
        AdminUpdateAvatarCommand command,
        CancellationToken cancellationToken
    )
    {
        // Get admin user from repository using userId from JWT claims
        UserEntity? user = await userRepository.GetUserWithRolesAndPermissionsByIdAsync(
            command.UserId,
            cancellationToken
        );

        // Ensure the account is active (admin users only need to be active, not verified)
        userRepository.IsUserAccountActive(user!);

        // Update user avatar using the user service (this handles the file management)
        user = await userService.UpdateUserAvatarAsync(user!, command.AvatarUrl, cancellationToken);

        await userRepository.SaveChangesAsync(cancellationToken);

        // Extract roles and permissions using repository
        var (roles, permissions) = roleRepository.GetUserRolesAndPermissions(user.UserRoles);

        // Fetch the avatar file if the user has one
        FileEntity? avatarFile = await fileRepository.GetAvatarFileAsync(user.AvatarFileId, cancellationToken);

        var avatarDto = avatarFile?.ToFileDto();
        var userDto = user.ToUserResponseDto(roles, permissions, avatarDto);

        return new AdminUpdateAvatarResult(userDto);
    }
}
