using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.Login.Contracts;
using _116.Identity.Application.Session.Factories;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Domain.Results;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Auth.UseCases.Admin.Commands.Login;

/// <summary>
/// Handles the <see cref="AdminLoginCommand" /> to authenticate admin users.
/// </summary>
/// <param name="authFactory">Factory for handling admin user authentication logic.</param>
/// <param name="sessionFactory">Factory for creating authentication sessions.</param>
/// <param name="fileRepository">Repository for accessing file metadata.</param>
public class AdminLoginHandler(
    IAdminLoginAuthFactory authFactory,
    ISessionFactory sessionFactory,
    IFileRepository fileRepository
) : ICommandHandler<AdminLoginCommand, AdminLoginResult>
{
    /// <summary>
    /// Handles the admin login command by authenticating the user and validating admin privileges.
    /// </summary>
    public async Task<AdminLoginResult> Handle(AdminLoginCommand command, CancellationToken cancellationToken)
    {
        // Authenticate admin user and get associated data
        AdminLoginAuthData authData = await authFactory.AuthenticateAsync(
            email: command.Email,
            password: command.Password,
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

        // Build final result
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

        return new AdminLoginResult(AuthenticationResult: authResult);
    }
}
