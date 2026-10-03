using _116.Identity.Application.Adapters.SocialAuth;
using _116.Identity.Application.Auth.UseCases.Public.Commands.SocialLogin.Contracts;
using _116.Identity.Application.Shared.Errors.Facade;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Application.User.Ports;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Storage.Contracts.Application.Services;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.SocialLogin;

/// <summary>
/// Service implementation for handling social authentication logic.
/// </summary>
/// <param name="verifierFactory">Factory selecting the verifier for the provider.</param>
/// <param name="authRepository">Repository for user data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="i18n">Single i18n entry point for the Identity module.</param>
public class PublicSocialLoginAuthService(
    ISocialTokenVerifierFactory verifierFactory,
    IAuthRepository authRepository,
    IAvatarService avatarService,
    IIdentityUnitOfWork unitOfWork,
    IdentityI18n i18n
) : IPublicSocialLoginAuthService
{
    /// <inheritdoc />
    public async Task<PublicSocialLoginAuthData> AuthenticateAsync(
        EnumAuthProvider provider,
        string idToken,
        CancellationToken cancellationToken
    )
    {
        // An unsupported provider or an unverifiable token surfaces as an exception mapped by the global pipeline.
        ISocialTokenVerifier verifier = verifierFactory.For(provider: provider);
        SocialTokenPayload payload = await verifier.VerifyAsync(idToken: idToken, cancellationToken: cancellationToken);

        if (!payload.EmailVerified || string.IsNullOrWhiteSpace(value: payload.Email))
        {
            throw i18n.User.ProviderEmailNotVerified();
        }

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
