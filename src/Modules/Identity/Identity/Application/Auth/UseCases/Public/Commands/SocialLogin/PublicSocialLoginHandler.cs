using _116.BuildingBlocks.Constants;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Session.Repositories;
using _116.Identity.Application.Session.Services;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.Results;
using _116.Identity.Domain.ValueObjects;
using _116.Shared.Contracts.Application.CQRS;

using Microsoft.AspNetCore.Http;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.SocialLogin;

/// <summary>
/// Handles the <see cref="PublicSocialLoginCommand" /> for social authentication.
/// </summary>
/// <param name="authRepository">Repository for user data access operations.</param>
/// <param name="roleRepository">Repository for role and permission data operations.</param>
/// <param name="jwtService">Service for generating JWT tokens with user claims.</param>
/// <param name="refreshTokenService">Service for generating and hashing refresh tokens.</param>
/// <param name="sessionRepository">Repository for managing user sessions.</param>
/// <param name="sessionMetadataService">Service for extracting session metadata from HTTP context.</param>
/// <param name="httpContextAccessor">Accessor for HTTP context.</param>
/// <param name="fileRepository">Repository for accessing file metadata.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class PublicSocialLoginHandler(
    IAuthRepository authRepository,
    IRoleRepository roleRepository,
    IJwtService jwtService,
    IRefreshTokenService refreshTokenService,
    ISessionRepository sessionRepository,
    ISessionMetadataService sessionMetadataService,
    IHttpContextAccessor httpContextAccessor,
    IFileRepository fileRepository,
    IIdentityUnitOfWork unitOfWork
) : ICommandHandler<PublicSocialLoginCommand, PublicSocialLoginResult>
{
    /// <summary>
    /// Handles the social login command by finding or creating a user account.
    /// </summary>
    /// <param name="command">The social login command containing provider data.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="PublicSocialLoginResult" /> containing authentication information.</returns>
    public async Task<PublicSocialLoginResult> Handle(
        PublicSocialLoginCommand command,
        CancellationToken cancellationToken
    )
    {
        // Normalize email and provider using value objects
        var email = new Email(value: command.Email);
        var provider = new AuthProvider(value: command.Provider);
        // Get or create external user for social authentication
        UserEntity? user = await authRepository.GetOrCreateExternalUserAsync(
            email: email.Value,
            userName: command.UserName,
            authProvider: provider,
            cancellationToken: cancellationToken
        );
        // Update avatar from provider URL if allowed
        bool isAvatarSourceManual = user!.AvatarSource == EnumAvatarSource.Manual;
        FileEntity? avatarFileEntity = await fileRepository.UpdateAvatarUrlFromSourceAsync(
            currentAvatarFileId: user.AvatarFileId,
            avatarUrl: command.AvatarUrl,
            user.Id.ToString(),
            isAvatarSourceManual: isAvatarSourceManual,
            cancellationToken: cancellationToken
        );
        if (avatarFileEntity != null)
        {
            user.UpdateAvatar(avatarFileId: avatarFileEntity.Id, avatarSource: EnumAvatarSource.Provider);
        }

        // Save user changes
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
        // Extract user permissions for JWT
        List<RolePermissionEntity> userPermissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .ToList();
        // Generate JWT access token with user claims
        JwtGenerationResult accessToken = jwtService.GenerateToken(
            userId: user.Id,
            user.Email!,
            userName: user.UserName,
            userRoles: user.UserRoles,
            userPermissions: userPermissions,
            isVerified: user.IsVerified,
            isActive: user.IsActive,
            authProvider: provider.Value
        );
        // Generate refresh token and create session
        string refreshToken = refreshTokenService.GenerateRefreshToken();
        string refreshTokenHash = refreshTokenService.HashRefreshToken(refreshToken: refreshToken);
        DateTime refreshTokenExpiresAt = DateTime.UtcNow.AddDays(value: SessionConstants.RefreshTokenExpirationDays);
        // Extract session metadata from HTTP context
        HttpContext? httpContext = httpContextAccessor.HttpContext;
        string? ipAddress = sessionMetadataService.ExtractIpAddress(httpContext: httpContext);
        string? userAgent = sessionMetadataService.ExtractUserAgent(httpContext: httpContext);
        string? deviceName = sessionMetadataService.ParseDeviceName(userAgent: userAgent);
        var session = SessionEntity.Create(
            Guid.NewGuid(),
            userId: user.Id,
            refreshTokenHash: refreshTokenHash,
            expiresAt: refreshTokenExpiresAt,
            ipAddress: ipAddress,
            userAgent: userAgent,
            deviceName: deviceName
        );
        await sessionRepository.CreateAsync(session: session, cancellationToken: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
        // Extract roles and permissions using repository
        var (roles, permissions) = roleRepository.GetUserRolesAndPermissions(userRoles: user.UserRoles);
        // Fetch the avatar file if the user has one
        FileEntity? avatarFile =
            await fileRepository.GetAvatarFileAsync(avatarFileId: user.AvatarFileId,
                cancellationToken: cancellationToken);
        // Map to userDTO with avatar and create the result
        var avatarDto = avatarFile?.ToFileDto();
        var userDto = user.ToUserResponseDto(roles: roles, permissions: permissions, avatar: avatarDto);
        var authResult = new AuthenticationResult(
            User: userDto,
            AccessToken: accessToken.Token,
            AccessTokenExpiresAt: accessToken.ExpiresAt,
            RefreshToken: refreshToken,
            RefreshTokenExpiresAt: refreshTokenExpiresAt
        );
        return new PublicSocialLoginResult(AuthenticationResult: authResult);
    }
}
