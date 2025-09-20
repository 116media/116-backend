using _116.Core.Application.Shared.Repositories;
using _116.Core.Application.Shared.Services;
using _116.Core.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using _116.User.Application.Shared.Mappers;
using _116.User.Application.Shared.Repositories;
using _116.User.Application.Shared.Services;
using _116.User.Domain.Entities;

namespace _116.User.Application.Public.UseCases.Commands.UpdateAvatar;

/// <summary>
/// Handles the <see cref="PublicUpdateAvatarCommand"/> to update user avatar.
/// </summary>
/// <param name="userRepository">Repository for user data access operations.</param>
/// <param name="fileRepository">Repository for file data access operations.</param>
/// <param name="userService">Service for user operations including avatar updates.</param>
public class PublicUpdateAvatarHandler(
    IUserRepository userRepository,
    IFileRepository fileRepository,
    IRoleRepository roleRepository,
    IUserService userService
) : ICommandHandler<PublicUpdateAvatarCommand, PublicUpdateAvatarResult>
{
    /// <summary>
    /// Handles the avatar update command by updating the user's avatar URL.
    /// </summary>
    /// <param name="command">The avatar update command containing the user ID and new avatar URL.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The result containing the updated user information.</returns>
    public async Task<PublicUpdateAvatarResult> Handle(PublicUpdateAvatarCommand command, CancellationToken cancellationToken)
    {
        UserEntity? user = await userRepository.GetUserWithRolesAndPermissionsByIdAsync(
            command.UserId,
            cancellationToken
        );

        // Ensure the account is active and verified (public users must be verified)
        userRepository.IsUserAccountActive(user!);
        userRepository.IsUserAccountVerified(user!);

        // Update user avatar using the user service (this handles the file management)
        user = await userService.UpdateUserAvatarAsync(user!, command.AvatarUrl, cancellationToken);

        await userRepository.SaveChangesAsync(cancellationToken);

        // Extract roles and permissions using repository
        var (roles, permissions) = roleRepository.GetUserRolesAndPermissions(user.UserRoles);

        // Fetch the avatar file if the user has one
        FileEntity? avatarFile = await fileRepository.GetAvatarFileAsync(user.AvatarFileId, cancellationToken);

        var avatarDto = avatarFile?.ToFileDto();
        var userDto = user.ToUserResponseDto(roles, permissions, avatarDto);

        return new PublicUpdateAvatarResult(userDto);
    }
}
