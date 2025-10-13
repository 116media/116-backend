using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Auth.Application.Shared.Persistence;
using _116.Shared.Contracts.Application.CQRS;
using _116.Auth.Application.Shared.Mappers;
using _116.Auth.Application.Shared.Repositories;
using _116.Auth.Application.Shared.Services;
using _116.Auth.Domain.Entities;
using _116.Auth.Domain.Results;
using _116.Auth.Domain.ValueObjects;

namespace _116.Auth.Application.Public.UseCases.Commands.SocialLogin;

/// <summary>
/// Handles the <see cref="PublicSocialLoginCommand"/> for social authentication.
/// </summary>
/// <param name="userService">Service for user management operations.</param>
/// <param name="roleRepository">Repository for role and permission data operations.</param>
/// <param name="jwtService">Service for generating JWT tokens with user claims.</param>
/// <param name="fileRepository">Repository for accessing file metadata.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class PublicSocialLoginHandler(
    IUserService userService,
    IRoleRepository roleRepository,
    IJwtService jwtService,
    IFileRepository fileRepository,
    IAuthUnitOfWork unitOfWork
) : ICommandHandler<PublicSocialLoginCommand, PublicSocialLoginResult>
{
    /// <summary>
    /// Handles the social login command by finding or creating a user account.
    /// </summary>
    /// <param name="command">The social login command containing provider data.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="PublicSocialLoginResult"/> containing authentication information.</returns>
    public async Task<PublicSocialLoginResult> Handle(
        PublicSocialLoginCommand command,
        CancellationToken cancellationToken
    )
    {
        // Normalize email and provider using value objects
        var email = new Email(command.Email);
        var provider = new AuthProvider(command.Provider);

        // Get or create external user for social authentication
        UserEntity? user = await userService.GetOrCreateExternalUserAsync(
            email.Value,
            command.UserName,
            provider.Value,
            cancellationToken
        );

        // Update user avatar if provided
        user = await userService.UpdateUserAvatarAsync(user!, command.AvatarUrl, cancellationToken);

        // Record login and save changes
        user.RecordLogin();
        await unitOfWork.CommitAsync(cancellationToken);

        // Extract user permissions for JWT
        List<RolePermissionEntity> userPermissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .ToList();

        // Generate JWT token with user claims
        JwtGenerationResult token = jwtService.GenerateToken(
            userId: user.Id,
            email: user.Email!,
            userName: user.UserName,
            userRoles: user.UserRoles,
            userPermissions: userPermissions,
            isVerified: user.IsVerified,
            isActive: user.IsActive,
            isLoggedIn: user.IsLoggedIn,
            authProvider: provider.Value
        );

        // Extract roles and permissions using repository
        var (roles, permissions) = roleRepository.GetUserRolesAndPermissions(user.UserRoles);

        // Fetch the avatar file if the user has one
        FileEntity? avatarFile = await fileRepository.GetAvatarFileAsync(user.AvatarFileId, cancellationToken);

        // Map to userDTO with avatar and create the result
        var avatarDto = avatarFile?.ToFileDto();
        var userDto = user.ToUserResponseDto(roles, permissions, avatarDto);
        var authResult = new AuthenticationResult(userDto, token.Token, token.ExpiresAt);

        return new PublicSocialLoginResult(authResult);
    }
}
