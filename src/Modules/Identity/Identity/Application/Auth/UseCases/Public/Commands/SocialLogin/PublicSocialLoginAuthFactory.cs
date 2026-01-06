using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Identity.Application.Auth.UseCases.Public.Commands.SocialLogin.Contracts;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.ValueObjects;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.SocialLogin;

/// <summary>
/// Factory implementation for handling social authentication logic.
/// </summary>
/// <param name="authRepository">Repository for user data access operations.</param>
/// <param name="roleRepository">Repository for role and permission data operations.</param>
/// <param name="fileRepository">Repository for accessing file metadata.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class PublicSocialLoginAuthFactory(
    IAuthRepository authRepository,
    IRoleRepository roleRepository,
    IFileRepository fileRepository,
    IIdentityUnitOfWork unitOfWork
) : IPublicSocialLoginAuthFactory
{
    /// <summary>
    /// Authenticates or creates a user via social provider.
    /// </summary>
    public async Task<PublicSocialLoginAuthData> AuthenticateOrCreateAsync(
        string email,
        string userName,
        string provider,
        string? avatarUrl,
        CancellationToken cancellationToken
    )
    {
        UserEntity? user = await authRepository.GetOrCreateExternalUserAsync(
            userName: userName,
            email: new Email(value: email),
            authProvider: new AuthProvider(value: provider),
            cancellationToken: cancellationToken
        );

        // Update avatar from provider URL if allowed
        bool isAvatarSourceManual = user!.AvatarSource == EnumAvatarSource.Manual;
        FileEntity? avatarFileEntity = await fileRepository.UpdateAvatarUrlFromSourceAsync(
            currentAvatarFileId: user.AvatarFileId,
            avatarUrl: avatarUrl,
            user.Id.ToString(),
            isAvatarSourceManual: isAvatarSourceManual,
            cancellationToken: cancellationToken
        );

        if (avatarFileEntity != null)
        {
            user.UpdateAvatar(avatarFileId: avatarFileEntity.Id, avatarSource: EnumAvatarSource.Provider);
        }

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        List<RolePermissionEntity> userPermissions = user.UserRoles.SelectMany(ur => ur.Role.RolePermissions).ToList();

        var (roles, permissions) = roleRepository.GetUserRolesAndPermissions(userRoles: user.UserRoles);

        return new PublicSocialLoginAuthData(
            User: user,
            Roles: roles,
            Permissions: permissions,
            UserPermissions: userPermissions
        );
    }
}
