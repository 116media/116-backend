using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Application.User.UseCases.Admin.Commands.UpdateAvatar.Contracts;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Identity.Application.User.UseCases.Admin.Commands.UpdateAvatar;

/// <summary>
/// Handles the <see cref="AdminUpdateAvatarCommand" /> to update admin user avatar.
/// </summary>
/// <param name="authFactory">Factory for handling admin user avatar update logic.</param>
/// <param name="fileRepository">Repository for file data access operations.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminUpdateAvatarHandler(
    IAdminUpdateAvatarAuthFactory authFactory,
    IFileRepository fileRepository,
    IMapper mapper
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
        AdminUpdateAvatarAuthData userData = await authFactory.GetUserForAvatarUpdateAsync(
            userId: command.UserId,
            sessionId: command.SessionId,
            cancellationToken: cancellationToken
        );

        FileEntity fileEntity = await fileRepository.UpdateAvatarFromFileAsync(
            currentAvatarFileId: userData.User.AvatarFileId,
            avatarFile: command.AvatarFile,
            command.UserId.ToString(),
            originalFileName: command.AvatarFile.FileName,
            mimeType: command.AvatarFile.ContentType,
            cancellationToken: cancellationToken
        );

        AdminUpdateAvatarAuthData authData = await authFactory.UpdateAvatarAsync(
            user: userData.User,
            avatarFileId: fileEntity.Id,
            cancellationToken: cancellationToken
        );

        FileEntity? avatarFile = await fileRepository.GetAvatarFileAsync(
            avatarFileId: authData.User.AvatarFileId,
            cancellationToken: cancellationToken
        );

        var avatarDto = avatarFile?.ToFileDto(mapper);
        var userDto = authData.User.ToUserResponseDto(
            mapper: mapper,
            roles: authData.Roles,
            permissions: authData.Permissions,
            avatar: avatarDto
        );
        return new AdminUpdateAvatarResult(User: userDto);
    }
}
