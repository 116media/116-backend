using _116.Core.Contracts.Application.Services;
using _116.Identity.Application.Adapters.SocialAuth;
using _116.Identity.Application.Auth.UseCases.Public.Commands.SocialLogin.Contracts;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Application.User.Services;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.SocialLogin;

/// <summary>
/// Factory implementation for handling social authentication logic.
/// </summary>
/// <param name="authRepository">Repository for user data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class PublicSocialLoginAuthFactory(
    IAuthRepository authRepository,
    IAvatarService avatarService,
    IIdentityUnitOfWork unitOfWork
) : IPublicSocialLoginAuthFactory
{
    /// <inheritdoc />
    public async Task<PublicSocialLoginAuthData> AuthenticateOrCreateAsync(
        SocialTokenPayload payload,
        EnumAuthProvider provider,
        CancellationToken cancellationToken
    )
    {
        UserEntity? user = await authRepository.GetOrCreateExternalUserAsync(
            email: payload.Email,
            userName: payload.Name ?? payload.Email,
            authProvider: provider,
            providerSubjectId: payload.ProviderSubjectId,
            cancellationToken: cancellationToken
        );

        bool hasManualAvatar = user!.AvatarSource == EnumAvatarSource.Manual;
        bool canAdoptProviderAvatar = !hasManualAvatar && !string.IsNullOrWhiteSpace(payload.PictureUrl);

        StoredFile? avatar = canAdoptProviderAvatar
            ? await avatarService.UploadFromUrlAsync(
                currentAvatarFileId: user.AvatarFileId,
                avatarUrl: payload.PictureUrl!,
                cancellationToken: cancellationToken
            )
            : null;

        await unitOfWork.ExecuteInTransactionAsync(
            async ct =>
            {
                if (avatar is not null)
                {
                    await avatarService.RecordAsync(
                        avatar: avatar,
                        supersededFileId: user.AvatarFileId,
                        cancellationToken: ct
                    );

                    user.UpdateAvatar(avatarFileId: avatar.Reference.Id, avatarSource: EnumAvatarSource.Provider);
                }
            },
            cancellationToken: cancellationToken
        );

        List<RolePermissionEntity> userPermissions = user.UserRoles.SelectMany(ur => ur.Role.RolePermissions).ToList();

        return new PublicSocialLoginAuthData(User: user, UserPermissions: userPermissions);
    }
}
