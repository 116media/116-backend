using _116.Identity.Application.Adapters.Wangkanai.Detection;
using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Auth.UseCases.Public.Commands.SocialLogin.Contracts;
using _116.Identity.Application.Session.Repositories;
using _116.Identity.Application.Session.Services;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.Results;
using _116.Shared.Application.Configurations;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.SocialLogin;

/// <summary>
/// Factory implementation for creating authentication sessions with tokens and metadata.
/// </summary>
/// <param name="jwtService">Service for generating JWT tokens with user claims.</param>
/// <param name="refreshTokenService">Service for generating and hashing refresh tokens.</param>
/// <param name="sessionRepository">Repository for managing user sessions.</param>
/// <param name="sessionMetadataService">Service for extracting session metadata from HTTP context.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class PublicSocialLoginSessionFactory(
    IJwtService jwtService,
    IRefreshTokenService refreshTokenService,
    ISessionRepository sessionRepository,
    ISessionMetadataService sessionMetadataService,
    IIdentityUnitOfWork unitOfWork
) : IPublicSocialLoginSessionFactory
{
    /// <summary>
    /// Creates a new authentication session for a user.
    /// </summary>
    public async Task<PublicSocialLoginSessionData> CreateSessionAsync(
        UserEntity user,
        List<RolePermissionEntity> userPermissions,
        CancellationToken cancellationToken
    )
    {
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

        var (_, _, _, _, refreshTokenExpirationMinutes) = AppEnvironment.Jwt();

        string refreshToken = refreshTokenService.GenerateRefreshToken();
        string refreshTokenHash = refreshTokenService.HashRefreshToken(refreshToken: refreshToken);
        DateTime refreshTokenExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(refreshTokenExpirationMinutes!));

        string? ipAddress = sessionMetadataService.ExtractIpAddress();
        string? userAgent = sessionMetadataService.ExtractUserAgent();
        ClientOriginInfo clientOrigin = sessionMetadataService.GetClientOriginInfo();
        EnumClient clientApp = sessionMetadataService.ExtractClientApp();

        var session = SessionEntity.Create(
            id: Guid.NewGuid(),
            userId: user.Id,
            refreshTokenHash: refreshTokenHash,
            expiresAt: refreshTokenExpiresAt,
            browser: clientOrigin.Browser,
            device: clientOrigin.Device,
            platform: clientOrigin.Platform,
            client: clientApp,
            ipAddress: ipAddress,
            userAgent: userAgent
        );

        // Persist session
        await sessionRepository.CreateAsync(session: session, cancellationToken: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new PublicSocialLoginSessionData(
            RefreshToken: refreshToken,
            AccessToken: accessToken.Token,
            AccessTokenExpiresAt: accessToken.ExpiresAt,
            RefreshTokenExpiresAt: refreshTokenExpiresAt
        );
    }
}
