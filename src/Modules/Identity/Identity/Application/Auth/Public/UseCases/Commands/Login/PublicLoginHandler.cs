using _116.BuildingBlocks.Constants;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Identity.Application.Shared.Errors;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Application.Shared.Services;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Results;
using _116.Shared.Application.Exceptions;
using _116.Shared.Contracts.Application.CQRS;

using Microsoft.AspNetCore.Http;

namespace _116.Identity.Application.Auth.Public.UseCases.Commands.Login;

/// <summary>
/// Handles the <see cref="PublicLoginCommand"/> to authenticate public users.
/// </summary>
/// <param name="authRepository">Repository for user data access operations.</param>
/// <param name="roleRepository">Repository for role and permission data operations.</param>
/// <param name="passwordService">Service for verifying hashed passwords.</param>
/// <param name="jwtService">Service for generating JWT tokens with user claims.</param>
/// <param name="refreshTokenService">Service for generating and hashing refresh tokens.</param>
/// <param name="sessionRepository">Repository for managing user sessions.</param>
/// <param name="sessionMetadataService">Service for extracting session metadata from HTTP context.</param>
/// <param name="httpContextAccessor">Accessor for HTTP context.</param>
/// <param name="fileRepository">Repository for accessing file metadata.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class PublicLoginHandler(
    IAuthRepository authRepository,
    IRoleRepository roleRepository,
    IPasswordService passwordService,
    IJwtService jwtService,
    IRefreshTokenService refreshTokenService,
    ISessionRepository sessionRepository,
    ISessionMetadataService sessionMetadataService,
    IHttpContextAccessor httpContextAccessor,
    IFileRepository fileRepository,
    IIdentityUnitOfWork unitOfWork
) : ICommandHandler<PublicLoginCommand, PublicLoginResult>
{
    /// <summary>
    /// Handles the public login command by authenticating the user with email or username.
    /// </summary>
    /// <param name="command">The public login command containing credentials.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="PublicLoginResult"/> containing public user authentication information.</returns>
    /// <exception cref="NotFoundException">Thrown when no user is found with the specified credentials.</exception>
    /// <exception cref="BadRequestException">Thrown when password is invalid.</exception>
    /// <exception cref="AuthorizationException">
    /// Thrown when the user account is inactive or not verified (HTTP 403 Forbidden).
    /// </exception>
    public async Task<PublicLoginResult> Handle(PublicLoginCommand command, CancellationToken cancellationToken)
    {
        UserEntity? user = await authRepository.GetUserWithRolesAndPermissionsByCredentialsOrThrow(
            command.Credentials,
            cancellationToken
        );
        // Verify password first before revealing account status
        if (!passwordService.Verify(command.Password, user!.PasswordHash))
        {
            throw UserErrors.InvalidCredentials();
        }
        // Validate user can login
        user.ValidateCanLogin();
        // Extract user permissions from roles (already loaded by repository)
        List<RolePermissionEntity> userPermissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .ToList();
        // Generate JWT access token with user claims
        JwtGenerationResult accessToken = jwtService.GenerateToken(
            userId: user.Id,
            email: user.Email!,
            userName: user.UserName,
            userRoles: user.UserRoles,
            userPermissions: userPermissions,
            isVerified: user.IsVerified,
            isActive: user.IsActive,
            authProvider: user.AuthProvider
        );
        // Generate refresh token and create session
        string refreshToken = refreshTokenService.GenerateRefreshToken();
        string refreshTokenHash = refreshTokenService.HashRefreshToken(refreshToken);
        DateTime refreshTokenExpiresAt = DateTime.UtcNow.AddDays(SessionConstants.RefreshTokenExpirationDays);
        // Extract session metadata from HTTP context
        HttpContext? httpContext = httpContextAccessor.HttpContext;
        string? ipAddress = sessionMetadataService.ExtractIpAddress(httpContext);
        string? userAgent = sessionMetadataService.ExtractUserAgent(httpContext);
        string? deviceName = sessionMetadataService.ParseDeviceName(userAgent);
        SessionEntity session = SessionEntity.Create(
            id: Guid.NewGuid(),
            userId: user.Id,
            refreshTokenHash: refreshTokenHash,
            expiresAt: refreshTokenExpiresAt,
            ipAddress: ipAddress,
            userAgent: userAgent,
            deviceName: deviceName
        );
        await sessionRepository.CreateAsync(session, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        // Extract roles and permissions using repository
        var (roles, permissions) = roleRepository.GetUserRolesAndPermissions(user.UserRoles);
        // Fetch the avatar file if the user has one
        FileEntity? avatarFile = await fileRepository.GetAvatarFileAsync(user.AvatarFileId, cancellationToken);
        // Map to userDTO with avatar
        var avatarDto = avatarFile?.ToFileDto();
        var userDto = user.ToUserResponseDto(roles, permissions, avatarDto);
        var authResult = new AuthenticationResult(
            userDto,
            accessToken.Token,
            accessToken.ExpiresAt,
            refreshToken,
            refreshTokenExpiresAt
        );
        return new PublicLoginResult(authResult);
    }
}
