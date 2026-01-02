using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Identity.Application.Auth.UseCases.Public.Commands.Login.Contracts;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Domain.Results;
using _116.Shared.Application.Exceptions;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.Login;

/// <summary>
/// Handles the <see cref="PublicLoginCommand" /> to authenticate public users.
/// </summary>
/// <param name="authFactory">Factory for handling user authentication logic.</param>
/// <param name="sessionFactory">Factory for creating authentication sessions.</param>
/// <param name="fileRepository">Repository for accessing file metadata.</param>
public class PublicLoginHandler(
    IPublicLoginAuthFactory authFactory,
    IPublicLoginSessionFactory sessionFactory,
    IFileRepository fileRepository
) : ICommandHandler<PublicLoginCommand, PublicLoginResult>
{
    /// <summary>
    /// Handles the public login command by authenticating the user with email or username.
    /// </summary>
    /// <param name="command">The public login command containing credentials.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="PublicLoginResult" /> containing public user authentication information.</returns>
    /// <exception cref="NotFoundException">Thrown when no user is found with the specified credentials.</exception>
    /// <exception cref="BadRequestException">Thrown when password is invalid.</exception>
    /// <exception cref="AuthorizationException">
    /// Thrown when the user account is inactive or not verified (HTTP 403 Forbidden).
    /// </exception>
    public async Task<PublicLoginResult> Handle(PublicLoginCommand command, CancellationToken cancellationToken)
    {
        // Authenticate user and get associated data
        PublicLoginAuthData authData = await authFactory.AuthenticateAsync(
            credentials: command.Credentials,
            password: command.Password,
            cancellationToken: cancellationToken
        );

        // Create authentication session with tokens
        PublicLoginSessionData sessionData = await sessionFactory.CreateSessionAsync(
            user: authData.User,
            userPermissions: authData.UserPermissions,
            cancellationToken: cancellationToken
        );

        // Fetch user avatar
        FileEntity? avatarFile = await fileRepository.GetAvatarFileAsync(
            avatarFileId: authData.User.AvatarFileId,
            cancellationToken: cancellationToken
        );

        var avatarDto = avatarFile?.ToFileDto();
        var userDto = authData.User.ToUserResponseDto(
            roles: authData.Roles,
            permissions: authData.Permissions,
            avatar: avatarDto
        );

        var authResult = new AuthenticationResult(
            User: userDto,
            AccessToken: sessionData.AccessToken,
            AccessTokenExpiresAt: sessionData.AccessTokenExpiresAt,
            RefreshToken: sessionData.RefreshToken,
            RefreshTokenExpiresAt: sessionData.RefreshTokenExpiresAt
        );

        return new PublicLoginResult(AuthenticationResult: authResult);
    }
}
