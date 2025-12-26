using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.User.UseCases.Admin.Commands.UpdateAvatar;

/// <summary>
/// Handles the <see cref="AdminUpdateAvatarCommand" /> to update admin user avatar.
/// </summary>
/// <param name="authRepository">Repository for user data access operations.</param>
/// <param name="fileRepository">Repository for file data access operations.</param>
/// <param name="roleRepository">Repository for role and permission data operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminUpdateAvatarHandler(
    IAuthRepository authRepository,
    IFileRepository fileRepository,
    IRoleRepository roleRepository,
    IIdentityUnitOfWork unitOfWork
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
        UserEntity? user = await authRepository.GetUserWithRolesAndPermissionsByIdOrThrow(
            userId: command.UserId,
            cancellationToken: cancellationToken
        );
        // Ensure the account is active (admin users only need to be active)
        authRepository.IsUserAccountActive(user!);
        authRepository.IsUserLoggedIn(user!);

        // Update avatar (deletes old and uploads new)
        FileEntity fileEntity = await fileRepository.UpdateAvatarFromFileAsync(
            currentAvatarFileId: user!.AvatarFileId,
            avatarFile: command.AvatarFile,
            user.Id.ToString(),
            originalFileName: command.AvatarFile.FileName,
            mimeType: command.AvatarFile.ContentType,
            cancellationToken: cancellationToken
        );
        // Update user with new avatar file ID
        user.UpdateAvatar(avatarFileId: fileEntity.Id, avatarSource: EnumAvatarSource.Manual);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
        // Extract roles and permissions using repository
        var (roles, permissions) = roleRepository.GetUserRolesAndPermissions(userRoles: user.UserRoles);
        // Fetch the avatar file if the user has one
        FileEntity? avatarFile =
            await fileRepository.GetAvatarFileAsync(avatarFileId: user.AvatarFileId,
                cancellationToken: cancellationToken);
        var avatarDto = avatarFile?.ToFileDto();
        var userDto = user.ToUserResponseDto(roles: roles, permissions: permissions, avatar: avatarDto);
        return new AdminUpdateAvatarResult(User: userDto);
    }
}
