using _116.Core.Application.Shared.Repositories;
using _116.Core.Application.Shared.Services;
using _116.Core.Domain.Entities;
using _116.Identity.Application.Adapters.SocialAuth;
using _116.Identity.Application.Auth.UseCases.Public.Commands.SocialLogin.Contracts;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.SocialLogin;

/// <summary>
/// Factory implementation for handling social authentication logic.
/// </summary>
/// <param name="authRepository">Repository for user data access operations.</param>
/// <param name="fileUploadService">Uploads and replaces stored assets.</param>
/// <param name="fileRepository">Repository for accessing file metadata.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class PublicSocialLoginAuthFactory(
    IAuthRepository authRepository,
    IFileRepository fileRepository,
    IFileUploadService fileUploadService,
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

        FileEntity? avatarFileEntity = null;
        bool hasManualAvatar = user!.AvatarSource == EnumAvatarSource.Manual;
        if (!hasManualAvatar && !string.IsNullOrWhiteSpace(payload.PictureUrl))
        {
            avatarFileEntity = await fileUploadService.UpdateAvatarFromUrlAsync(
                currentAvatarFileId: user.AvatarFileId,
                newAvatarUrl: payload.PictureUrl!,
                userId: user.Id.ToString(),
                cancellationToken: cancellationToken
            );
        }

        if (avatarFileEntity != null)
        {
            user.UpdateAvatar(avatarFileId: avatarFileEntity.Id, avatarSource: EnumAvatarSource.Provider);
        }

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        if (avatarFileEntity is not null)
        {
            await fileRepository.ClaimAsync(fileId: avatarFileEntity.Id, cancellationToken: cancellationToken);
        }

        List<RolePermissionEntity> userPermissions = user.UserRoles.SelectMany(ur => ur.Role.RolePermissions).ToList();

        return new PublicSocialLoginAuthData(User: user, UserPermissions: userPermissions);
    }
}
