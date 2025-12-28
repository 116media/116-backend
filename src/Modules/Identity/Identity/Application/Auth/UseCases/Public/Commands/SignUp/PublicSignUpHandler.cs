using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Auth.Repositories;
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
using _116.Shared.Application.Exceptions;
using _116.Shared.Contracts.Application.CQRS;

using Microsoft.AspNetCore.Http;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.SignUp;

/// <summary>
/// Handles the <see cref="PublicSignUpCommand" /> to register new public users.
/// </summary>
/// <param name="authRepository">Repository for user data access operations.</param>
/// <param name="roleRepository">Repository for role and permission data operations.</param>
/// <param name="otpRepository">Repository for OTP data access operations.</param>
/// <param name="passwordService">Service for hashing passwords.</param>
/// <param name="otpService">Service for generating OTP codes.</param>
/// <param name="jwtService">Service for generating JWT tokens with user claims.</param>
/// <param name="refreshTokenService">Service for generating and hashing refresh tokens.</param>
/// <param name="sessionRepository">Repository for managing user sessions.</param>
/// <param name="sessionMetadataService">Service for extracting session metadata from HTTP context.</param>
/// <param name="httpContextAccessor">Accessor for HTTP context.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class PublicSignUpHandler(
    IAuthRepository authRepository,
    IRoleRepository roleRepository,
    IOtpRepository otpRepository,
    IPasswordService passwordService,
    IOtpService otpService,
    IJwtService jwtService,
    IRefreshTokenService refreshTokenService,
    ISessionRepository sessionRepository,
    ISessionMetadataService sessionMetadataService,
    IHttpContextAccessor httpContextAccessor,
    IIdentityUnitOfWork unitOfWork
) : ICommandHandler<PublicSignUpCommand, PublicSignUpResult>
{
    /// <summary>
    /// Handles the public sign-up command by creating a new user account.
    /// </summary>
    /// <param name="command">The public sign-up command containing user registration data.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="PublicSignUpResult" /> containing user authentication information.</returns>
    /// <exception cref="ConflictException">Thrown when email or username already exists.</exception>
    public async Task<PublicSignUpResult> Handle(PublicSignUpCommand command, CancellationToken cancellationToken)
    {
        // Normalize email using value object
        var email = new Email(value: command.Email);
        // Validate unique credentials (email and username) in single repository call
        await authRepository.ValidateUniqueCredentialsAsync(email: email, userName: command.UserName,
            cancellationToken: cancellationToken);
        // Hash the password
        string hashedPassword = passwordService.Hash(password: command.Password);
        // Create new user entity
        var newUser = UserEntity.Create(
            Guid.NewGuid(),
            email: email.Value,
            userName: command.UserName,
            passwordHash: hashedPassword
        );
        // Add user to repository
        await authRepository.AddAsync(user: newUser, cancellationToken: cancellationToken);
        // Assign the visitor role to the new user
        await authRepository.AssignVisitorRoleAsync(userId: newUser.Id, cancellationToken: cancellationToken);
        // Generate OTP for email verification
        OtpEntity verificationOtp = otpService.CreateOtp(userId: newUser.Id, purpose: EnumOtpPurpose.EmailVerification);
        await otpRepository.AddAsync(otp: verificationOtp, cancellationToken: cancellationToken);
        // Save all changes atomically in a single transaction
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
        // Get the newly created user with roles to generate token
        UserEntity? userWithRoles = await authRepository.GetUserWithRolesAndPermissionsByCredentialsOrThrow(
            credentials: email.Value,
            cancellationToken: cancellationToken
        );
        // Extract user permissions from roles (already loaded by repository)
        List<RolePermissionEntity> userPermissions = userWithRoles!.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .ToList();
        // Generate JWT access token with user claims
        JwtGenerationResult accessToken = jwtService.GenerateToken(
            userId: userWithRoles.Id,
            email: email.Value,
            userName: userWithRoles.UserName,
            userRoles: userWithRoles.UserRoles,
            userPermissions: userPermissions,
            isVerified: userWithRoles.IsVerified,
            isActive: userWithRoles.IsActive,
            authProvider: userWithRoles.AuthProvider
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
            userId: userWithRoles.Id,
            refreshTokenHash: refreshTokenHash,
            expiresAt: refreshTokenExpiresAt,
            ipAddress: ipAddress,
            userAgent: userAgent,
            deviceName: deviceName
        );
        await sessionRepository.CreateAsync(session: session, cancellationToken: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
        // Extract roles and permissions using repository
        var (roles, permissions) = roleRepository.GetUserRolesAndPermissions(userRoles: userWithRoles.UserRoles);
        // Map to userDTO
        var userDto = userWithRoles.ToUserResponseDto(roles: roles, permissions: permissions);
        var authResult = new AuthenticationResult(
            User: userDto,
            AccessToken: accessToken.Token,
            AccessTokenExpiresAt: accessToken.ExpiresAt,
            RefreshToken: refreshToken,
            RefreshTokenExpiresAt: refreshTokenExpiresAt
        );

        return new PublicSignUpResult(AuthenticationResult: authResult, true);
    }
}
