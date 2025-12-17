using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Identity.Application.Shared.Persistence;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Public.UseCases.Commands.UpdateAvatar;

/// <summary>
/// Handles the <see cref="PublicUpdateAvatarCommand"/> to update user avatar.
/// </summary>
/// <param name="userRepository">Repository for user data access operations.</param>
/// <param name="fileRepository">Repository for file data access operations.</param>
/// <param name="roleRepository">Repository for role and permission data operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class PublicUpdateAvatarHandler(
    IUserRepository userRepository,
    IFileRepository fileRepository,
    IRoleRepository roleRepository,
    IAuthUnitOfWork unitOfWork
) : ICommandHandler<PublicUpdateAvatarCommand, PublicUpdateAvatarResult>
{
    /// <summary>
    /// Handles the avatar update command by updating the user's avatar URL.
    /// </summary>
    /// <param name="command">The avatar update command containing the user ID and new avatar URL.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The result containing the updated user information.</returns>
    public async Task<PublicUpdateAvatarResult> Handle(
        PublicUpdateAvatarCommand command,
        CancellationToken cancellationToken
    )
    {
        UserEntity? user = await userRepository.GetUserWithRolesAndPermissionsByIdOrThrow(
            command.UserId,
            cancellationToken
        );

        // Validate user account status - must be active and verified
        userRepository.IsUserAccountActive(user!);
        userRepository.IsUserAccountVerified(user!);

        // Update avatar (deletes old and uploads new)
        FileEntity fileEntity = await fileRepository.UpdateAvatarFromFileAsync(
            currentAvatarFileId: user!.AvatarFileId,
            avatarFile: command.AvatarFile,
            userId: user.Id.ToString(),
            originalFileName: command.AvatarFile.FileName,
            mimeType: command.AvatarFile.ContentType,
            cancellationToken: cancellationToken
        );

        // Update user with new avatar file ID
        user.UpdateAvatar(fileEntity.Id, EnumAvatarSource.Manual);
        await unitOfWork.CommitAsync(cancellationToken);

        // Extract roles and permissions using repository
        var (roles, permissions) = roleRepository.GetUserRolesAndPermissions(user.UserRoles);

        // Fetch the avatar file if the user has one
        FileEntity? avatarFile = await fileRepository.GetAvatarFileAsync(user.AvatarFileId, cancellationToken);

        var avatarDto = avatarFile?.ToFileDto();
        var userDto = user.ToUserResponseDto(roles, permissions, avatarDto);

        return new PublicUpdateAvatarResult(userDto);
    }
}
