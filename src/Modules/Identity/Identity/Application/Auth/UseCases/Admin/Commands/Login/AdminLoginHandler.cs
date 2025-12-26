using _116.BuildingBlocks.Constants;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Session.Repositories;
using _116.Identity.Application.Session.Services;
using _116.Identity.Application.Shared.Errors;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Results;
using _116.Identity.Domain.ValueObjects;
using _116.Shared.Contracts.Application.CQRS;

using Microsoft.AspNetCore.Http;

namespace _116.Identity.Application.Auth.UseCases.Admin.Commands.Login;

/// <summary>
/// Handles the <see cref="AdminLoginCommand" /> to authenticate admin users.
/// </summary>
public class AdminLoginHandler(
    IAuthRepository authRepository,
    IRoleRepository roleRepository,
    IFileRepository fileRepository,
    IPasswordService passwordService,
    IJwtService jwtService,
    IRefreshTokenService refreshTokenService,
    ISessionRepository sessionRepository,
    ISessionMetadataService sessionMetadataService,
    IHttpContextAccessor httpContextAccessor,
    IIdentityUnitOfWork unitOfWork
) : ICommandHandler<AdminLoginCommand, AdminLoginResult>
{
    /// <summary>
    /// Handles the admin login command by authenticating the user and validating admin privileges.
    /// </summary>
    public async Task<AdminLoginResult> Handle(AdminLoginCommand command, CancellationToken cancellationToken)
    {
        // Normalize email using value object
        var email = new Email(value: command.Email);

        // Get admin user with all necessary data in one call
        UserEntity? user = await authRepository.GetUserWithRolesAndPermissionsByEmailOrThrow(
            email: email,
            cancellationToken: cancellationToken
        );

        // Verify password first before revealing account status
        if (!passwordService.Verify(password: command.Password, hash: user!.PasswordHash))
        {
            throw UserErrors.InvalidCredentials();
        }

        // Validate user can login
        user.ValidateCanLogin();

        // Verify admin has admin role
        authRepository.IsUserAdmin(user: user);

        // Extract user permissions from roles (already loaded by repository)
        List<RolePermissionEntity> userPermissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .ToList();

        // Generate JWT access token with admin claims
        JwtGenerationResult accessToken = jwtService.GenerateToken(
            userId: user.Id,
            user.Email!,
            userName: user.UserName,
            userRoles: user.UserRoles,
            userPermissions: userPermissions,
            isVerified: user.IsVerified,
            isActive: user.IsActive,
            authProvider: user.AuthProvider
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

        // Map to userDTO with avatar
        var avatarDto = avatarFile?.ToFileDto();
        var userDto = user.ToUserResponseDto(roles: roles, permissions: permissions, avatar: avatarDto);
        var authResult = new AuthenticationResult(
            User: userDto,
            AccessToken: accessToken.Token,
            AccessTokenExpiresAt: accessToken.ExpiresAt,
            RefreshToken: refreshToken,
            RefreshTokenExpiresAt: refreshTokenExpiresAt
        );

        return new AdminLoginResult(AuthenticationResult: authResult);
    }
}
