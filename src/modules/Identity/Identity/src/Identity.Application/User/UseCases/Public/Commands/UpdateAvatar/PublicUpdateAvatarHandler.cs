using _116.BuildingBlocks.Application.CQRS;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.User.Ports;
using _116.Identity.Application.User.UseCases.Public.Commands.UpdateAvatar.Contracts;
using _116.Storage.Contracts.Application.DTOs;
using _116.Storage.Contracts.Application.Services;
using MapsterMapper;
using Microsoft.AspNetCore.Http;

namespace _116.Identity.Application.User.UseCases.Public.Commands.UpdateAvatar;

/// <summary>
/// Handles the <see cref="PublicUpdateAvatarCommand" /> to update user avatar.
/// </summary>
/// <param name="authService">Service for handling user avatar update logic.</param>
/// <param name="avatarService">Resolves and stores the user's avatar.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class PublicUpdateAvatarHandler(
    IPublicUpdateAvatarAuthService authService,
    IAvatarService avatarService,
    IIdentityUnitOfWork unitOfWork,
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
        PublicUpdateAvatarAuthData userData = await authService.GetUserForAvatarUpdateAsync(
            userId: command.UserId,
            sessionId: command.SessionId,
            cancellationToken: cancellationToken
        );

        IFormFile file = command.AvatarFile!;

        StoredFile uploaded = await avatarService.UploadAsync(
            avatarFile: file,
            userId: command.UserId,
            cancellationToken: cancellationToken
        );

        PublicUpdateAvatarAuthData authData = await unitOfWork.ExecuteInTransactionAsync(
            async ct =>
            {
                await avatarService.RecordAsync(
                    avatar: uploaded,
                    supersededFileId: userData.User.AvatarFileId,
                    cancellationToken: ct
                );

                return await authService.UpdateAvatarAsync(
                    user: userData.User,
                    avatarFileId: uploaded.Reference.Id,
                    cancellationToken: ct
                );
            },
            cancellationToken: cancellationToken
        );

        FileDto? avatarDto = await avatarService.GetAvatarAsync(
            avatarFileId: authData.User.AvatarFileId,
            cancellationToken: cancellationToken
        );
        var userDto = authData.User.ToUserResponseDto(
            mapper: mapper,
            roles: authData.User.UserRoles.ToRoleDtos(mapper),
            permissions: authData.User.UserRoles.ToPermissionDtos(mapper),
            avatar: avatarDto
        );
        return new PublicUpdateAvatarResult(User: userDto.ToPublicUserResponseDto());
    }
}
