using _116.BuildingBlocks.Application.CQRS;
using _116.Identity.Application.Auth.UseCases.Public.Commands.SocialLogin.Contracts;
using _116.Identity.Application.Session.Services;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Application.User.Ports;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.ValueObjects;
using _116.Storage.Contracts.Application.DTOs;
using MapsterMapper;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.SocialLogin;

/// <summary>
/// Handles the <see cref="PublicSocialLoginCommand" /> to authenticate a user through a social
/// provider token and open a session.
/// </summary>
/// <param name="authService">Service verifying the token and resolving the user.</param>
/// <param name="sessionService">Service opening the session and issuing the tokens.</param>
/// <param name="avatarService">Service resolving the user's avatar.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class PublicSocialLoginHandler(
    IPublicSocialLoginAuthService authService,
    ISessionService sessionService,
    IAvatarService avatarService,
    IMapper mapper
) : ICommandHandler<PublicSocialLoginCommand, PublicSocialLoginResult>
{
    /// <inheritdoc />
    public async Task<PublicSocialLoginResult> Handle(
        PublicSocialLoginCommand command,
        CancellationToken cancellationToken
    )
    {
        EnumAuthProvider provider = new AuthProvider(value: command.Provider).Value;

        PublicSocialLoginAuthData authData = await authService.AuthenticateAsync(
            provider: provider,
            idToken: command.IdToken,
            cancellationToken: cancellationToken
        );

        SessionResult sessionData = await sessionService.CreateSessionAsync(
            user: authData.User,
            userPermissions: authData.UserPermissions,
            cancellationToken: cancellationToken
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
            AccessToken: sessionData.AccessToken,
            AccessTokenExpiresAt: sessionData.AccessTokenExpiresAt,
            RefreshToken: sessionData.RefreshToken,
            RefreshTokenExpiresAt: sessionData.RefreshTokenExpiresAt
        );

        return new PublicSocialLoginResult(Authentication: authResult);
    }
}
