using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Session.UseCases.Public.Commands.RefreshToken.Contracts;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Domain.Results;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Identity.Application.Session.UseCases.Public.Commands.RefreshToken;

/// <summary>
/// Handles the <see cref="PublicRefreshTokenCommand" /> to refresh access tokens.
/// </summary>
/// <param name="refreshTokenFactory">Factory for handling refresh token validation and rotation logic.</param>
/// <param name="jwtService">Service for generating JWT access tokens.</param>
/// <param name="fileRepository">Repository for accessing file metadata.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class PublicRefreshTokenHandler(
    IPublicRefreshTokenFactory refreshTokenFactory,
    IJwtService jwtService,
    IFileRepository fileRepository,
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
        PublicRefreshTokenData authData = await refreshTokenFactory.RefreshTokenAsync(
            refreshToken: command.RefreshToken,
            cancellationToken: cancellationToken
        );

        JwtGenerationResult jwtResult = jwtService.GenerateToken(
            userId: authData.User.Id,
            sessionId: authData.Session.Id,
            email: authData.User.Email!,
            userName: authData.User.UserName,
            userRoles: authData.User.UserRoles,
            userPermissions: authData.User.UserRoles.SelectMany(ur => ur.Role.RolePermissions).ToList(),
            isVerified: authData.User.IsVerified,
            isActive: authData.User.IsActive,
            authProvider: authData.User.AuthProvider
        );

        FileEntity? avatarFile = await fileRepository.GetAvatarFileAsync(
            avatarFileId: authData.User.AvatarFileId,
            cancellationToken: cancellationToken
        );

        var avatarDto = avatarFile?.ToFileDto(mapper);
        var userDto = authData.User.ToUserResponseDto(
            mapper: mapper,
            roles: authData.Roles,
            permissions: authData.Permissions,
            avatar: avatarDto
        );

        var authResult = new AuthenticationResult(
            User: userDto,
            AccessToken: jwtResult.Token,
            AccessTokenExpiresAt: jwtResult.ExpiresAt,
            RefreshToken: authData.NewRefreshToken,
            RefreshTokenExpiresAt: authData.Session.ExpiresAt
        );

        return new PublicRefreshTokenResult(AuthenticationResult: authResult);
    }
}
