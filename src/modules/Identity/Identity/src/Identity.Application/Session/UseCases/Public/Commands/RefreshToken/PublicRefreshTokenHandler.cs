using _116.BuildingBlocks.Application.CQRS;
using _116.Identity.Application.Auth.Ports;
using _116.Identity.Application.Session.Services;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Application.User.Ports;
using _116.Storage.Contracts.Application.DTOs;
using MapsterMapper;

namespace _116.Identity.Application.Session.UseCases.Public.Commands.RefreshToken;

/// <summary>
/// Handles the <see cref="PublicRefreshTokenCommand" /> to refresh access tokens.
/// </summary>
/// <param name="refreshTokenRotationService">Service for handling refresh token validation and rotation logic.</param>
/// <param name="jwtService">Service for generating JWT access tokens.</param>
/// <param name="avatarService">Resolves and stores the user's avatar.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class PublicRefreshTokenHandler(
    IRefreshTokenRotationService refreshTokenRotationService,
    IJwtService jwtService,
    IAvatarService avatarService,
    IMapper mapper
) : ICommandHandler<PublicRefreshTokenCommand, PublicRefreshTokenResult>
{
    /// <summary>
    /// Handles the refresh token command by validating and rotating the refresh token.
    /// </summary>
    /// <param name="command">The refresh token command containing the refresh token.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="PublicRefreshTokenResult" /> containing new authentication tokens.</returns>
    public async Task<PublicRefreshTokenResult> Handle(
        PublicRefreshTokenCommand command,
        CancellationToken cancellationToken
    )
    {
        RefreshTokenData authData = await refreshTokenRotationService.RefreshTokenAsync(
            refreshToken: command.RefreshToken,
            cancellationToken: cancellationToken
        );

        JwtGenerationDto jwtResult = jwtService.GenerateToken(
            userId: authData.User.Id,
            sessionId: authData.Session.Id,
            email: authData.User.Email!.Value,
            userName: authData.User.UserName,
            userRoles: authData.User.UserRoles,
            userPermissions: authData.User.UserRoles.SelectMany(ur => ur.Role.RolePermissions).ToList(),
            isVerified: authData.User.IsVerified,
            isActive: authData.User.IsActive,
            securityStamp: authData.TokenState.SecurityStamp,
            tokenVersion: authData.TokenState.TokenVersion,
            authProvider: authData.User.AuthProvider
        );

        FileDto? avatarDto = await avatarService.GetAvatarAsync(
            avatarFileId: authData.User.AvatarFileId,
            cancellationToken: cancellationToken
        );
        var userDto = authData.User.ToUserResponseDto(
            mapper: mapper,
            roles: authData.User.UserRoles.ToRoleDtos(mapper),
            permissions: authData.User.UserRoles.ToPermissionDtos(mapper),
            avatar: avatarDto
        );

        var authResult = new AuthenticationDto(
            User: userDto,
            AccessToken: jwtResult.Token,
            AccessTokenExpiresAt: jwtResult.ExpiresAt,
            RefreshToken: authData.NewRefreshToken,
            RefreshTokenExpiresAt: authData.Session.ExpiresAt
        );

        return new PublicRefreshTokenResult(Authentication: authResult);
    }
}
