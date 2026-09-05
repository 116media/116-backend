using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Adapters.Wangkanai.Detection;
using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Session.Factories.Contracts;
using _116.Identity.Application.Session.Repositories;
using _116.Identity.Application.Session.Services;
using _116.Identity.Application.Shared.Cache;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.Shared.Errors;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Shared.Application.Configurations;

namespace _116.Identity.Application.Session.Factories;

/// <summary>
/// Factory implementation for creating authentication sessions with tokens and metadata.
/// </summary>
/// <param name="jwtService">Service for generating JWT tokens with user claims.</param>
/// <param name="refreshTokenService">Service for generating and hashing refresh tokens.</param>
/// <param name="sessionRepository">Repository for managing user sessions.</param>
/// <param name="sessionMetadataService">Service for extracting session metadata from HTTP context.</param>
/// <param name="tokenStateRepository">Repository providing the user's token-invalidation markers.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="sessionErrors">Session domain error factory for generating domain exceptions.</param>
public class SessionFactory(
    IJwtService jwtService,
    IRefreshTokenService refreshTokenService,
    ISessionRepository sessionRepository,
    ISessionMetadataService sessionMetadataService,
    IUserTokenStateRepository tokenStateRepository,
    IIdentityUnitOfWork unitOfWork,
    SessionErrors sessionErrors
) : ISessionFactory
{
    /// <summary>
    /// Creates a new authentication session for a user or reuses an existing active session.
    /// If an active session already exists for the same device, it will be reused with rotated tokens.
    /// </summary>
    public async Task<SessionResult> CreateSessionAsync(
        UserEntity user,
        List<RolePermissionEntity> userPermissions,
        CancellationToken cancellationToken
    )
    {
        // Generate refresh token
        DateTimeOffset now = DateTimeOffset.UtcNow;
        var (_, _, _, _, refreshTokenExpirationMinutes) = AppEnvironment.Jwt();
        string refreshToken = refreshTokenService.GenerateRefreshToken();
        string refreshTokenHash = refreshTokenService.HashRefreshToken(refreshToken: refreshToken);
        DateTime refreshTokenExpiresAt = now.AddMinutes(minutes: int.Parse(refreshTokenExpirationMinutes!)).UtcDateTime;

        int absoluteLifetimeDays = AppEnvironment.SessionAbsoluteLifetimeDays(
            fallbackDays: SessionConstants.DefaultAbsoluteLifetimeDays
        );
        DateTime absoluteExpiresAt = now.AddDays(days: absoluteLifetimeDays).UtcDateTime;

        // Extract metadata
        string? deviceId = sessionMetadataService.ExtractDeviceId();
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            throw sessionErrors.DeviceIdRequired();
        }

        // Find any existing session for this device (active, expired, or revoked)
        SessionEntity? existingSession = await sessionRepository.GetSessionByUserIdAndDeviceIdAsync(
            userId: user.Id,
            deviceId: deviceId,
            cancellationToken: cancellationToken
        );

        Guid sessionId;
        if (existingSession != null)
        {
            sessionId = existingSession.Id;
            existingSession.Reactivate(refreshTokenHash, refreshTokenExpiresAt, absoluteExpiresAt);
        }
        else
        {
            sessionId = Guid.NewGuid();
            string? ipAddress = sessionMetadataService.ExtractIpAddress();
            string? userAgent = sessionMetadataService.ExtractUserAgent();
            ClientOriginInfo clientOrigin = sessionMetadataService.GetClientOriginInfo();
            EnumClient clientApp = sessionMetadataService.ExtractClientApp();

            // The reuse-or-create decision is made here: no session row exists
            // for this device, so the created session records a new device.
            var session = SessionEntity.Create(
                id: sessionId,
                userId: user.Id,
                deviceId: deviceId,
                refreshTokenHash: refreshTokenHash,
                expiresAt: refreshTokenExpiresAt,
                absoluteExpiresAt: absoluteExpiresAt,
                browser: clientOrigin.Browser,
                device: clientOrigin.Device,
                platform: clientOrigin.Platform,
                client: clientApp,
                ipAddress: ipAddress,
                userAgent: userAgent,
                isNewDevice: true
            );

            await sessionRepository.CreateAsync(session: session, cancellationToken: cancellationToken);
        }

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        UserSecurityState tokenState = await tokenStateRepository.GetOrCreateAsync(
            userId: user.Id,
            cancellationToken: cancellationToken
        );

        JwtGenerationDto accessToken = jwtService.GenerateToken(
            userId: user.Id,
            sessionId: sessionId,
            email: user.Email!,
            userName: user.UserName,
            userRoles: user.UserRoles,
            userPermissions: userPermissions,
            isVerified: user.IsVerified,
            isActive: user.IsActive,
            securityStamp: tokenState.SecurityStamp,
            tokenVersion: tokenState.TokenVersion,
            authProvider: user.AuthProvider
        );

        return new SessionResult(
            RefreshToken: refreshToken,
            AccessToken: accessToken.Token,
            AccessTokenExpiresAt: accessToken.ExpiresAt,
            RefreshTokenExpiresAt: refreshTokenExpiresAt
        );
    }
}
