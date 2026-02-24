using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Application.User.UseCases.Public.Commands.UpdateAvatar.Contracts;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Identity.Application.User.UseCases.Public.Commands.UpdateAvatar;

/// <summary>
/// Handles the <see cref="PublicUpdateAvatarCommand" /> to update user avatar.
/// </summary>
/// <param name="authFactory">Factory for handling user avatar update logic.</param>
/// <param name="fileRepository">Repository for file data access operations.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class PublicUpdateAvatarHandler(
    IPublicUpdateAvatarAuthFactory authFactory,
    IFileRepository fileRepository,
    IMapper mapper
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
        PublicUpdateAvatarAuthData userData = await authFactory.GetUserForAvatarUpdateAsync(
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

        PublicUpdateAvatarAuthData authData = await authFactory.UpdateAvatarAsync(
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
            roles: authData.User.UserRoles.ToRoleDtos(mapper),
            permissions: authData.User.UserRoles.ToPermissionDtos(mapper),
            avatar: avatarDto
        );
        return new PublicUpdateAvatarResult(User: userDto);
    }
}
