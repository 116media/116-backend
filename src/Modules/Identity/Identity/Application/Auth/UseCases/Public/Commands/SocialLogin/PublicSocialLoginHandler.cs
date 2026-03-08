using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Identity.Application.Auth.UseCases.Public.Commands.SocialLogin.Contracts;
using _116.Identity.Application.Session.Factories;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Domain.Results;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.SocialLogin;

/// <summary>
/// Handles the <see cref="PublicSocialLoginCommand" /> for social authentication.
/// </summary>
/// <param name="authFactory">Factory for handling social authentication logic.</param>
/// <param name="sessionFactory">Factory for creating authentication sessions.</param>
/// <param name="fileRepository">Repository for accessing file metadata.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class PublicSocialLoginHandler(
    IPublicSocialLoginAuthFactory authFactory,
    ISessionFactory sessionFactory,
    IFileRepository fileRepository,
    IMapper mapper
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
        // Authenticate or create user via social provider and get associated data
        PublicSocialLoginAuthData authData = await authFactory.AuthenticateOrCreateAsync(
            email: command.Email,
            userName: command.UserName,
            provider: command.Provider,
            avatarUrl: command.AvatarUrl,
            cancellationToken: cancellationToken
        );

        // Create authentication session with tokens
        SessionResult sessionData = await sessionFactory.CreateSessionAsync(
            user: authData.User,
            userPermissions: authData.UserPermissions,
            cancellationToken: cancellationToken
        );

        // Fetch user avatar
        FileEntity? avatarFile = await fileRepository.GetAvatarFileAsync(
            avatarFileId: authData.User.AvatarFileId,
            cancellationToken: cancellationToken
        );

        var avatarDto = avatarFile?.ToFileDto(mapper);
        var userDto = authData.User.ToUserResponseDto(
            mapper: mapper,
            roles: authData.User.UserRoles.ToRoleDtos(mapper),
            permissions: authData.User.UserRoles.ToPermissionDtos(mapper),
            avatar: avatarDto
        );

        var authResult = new AuthenticationResult(
            User: userDto,
            AccessToken: sessionData.AccessToken,
            AccessTokenExpiresAt: sessionData.AccessTokenExpiresAt,
            RefreshToken: sessionData.RefreshToken,
            RefreshTokenExpiresAt: sessionData.RefreshTokenExpiresAt
        );

        return new PublicSocialLoginResult(AuthenticationResult: authResult);
    }
}
