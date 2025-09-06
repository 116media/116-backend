using _116.Core.Application.Shared.Repositories;
using _116.Core.Application.Shared.Services;
using _116.Core.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Shared.Contracts.Application.CQRS;
using _116.User.Application.Shared.Errors;
using _116.User.Application.Shared.Mappers;
using _116.User.Application.Shared.Repositories;
using _116.User.Application.Shared.Services;
using _116.User.Domain.Entities;
using _116.User.Domain.Enums;
using _116.User.Domain.Results;
using _116.User.Domain.ValueObjects;

namespace _116.User.Application.Public.UseCases.Commands.SocialLogin;

/// <summary>
/// Handles the <see cref="SocialLoginCommand"/> for social authentication.
/// </summary>
/// <param name="userRepository">Repository for user data access operations.</param>
/// <param name="roleRepository">Repository for role and permission data operations.</param>
/// <param name="jwtService">Service for generating JWT tokens with user claims.</param>
/// <param name="fileService">Service for downloading and storing avatar files.</param>
/// <param name="fileRepository">Repository for accessing file metadata.</param>
public class SocialLoginHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IJwtService jwtService,
    IFileService fileService,
    IFileRepository fileRepository
) : ICommandHandler<SocialLoginCommand, SocialLoginResult>
{
    /// <summary>
    /// Handles the social login command by finding or creating a user account.
    /// </summary>
    /// <param name="command">The social login command containing provider data.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="SocialLoginResult"/> containing authentication information.</returns>
    /// <exception cref="ConflictException">Thrown when a local account exists with the same email.</exception>
    public async Task<SocialLoginResult> Handle(SocialLoginCommand command, CancellationToken cancellationToken)
    {
        // Normalize email and provider using value objects
        var email = new Email(command.Email);
        var provider = new Provider(command.Provider);

        UserEntity user;

        try
        {
            // Try to load existing user including roles and permissions
            user = await userRepository.GetPublicUserWithRolesAndPermissionsAsync(
                email.Value, cancellationToken
            );

            // Prevent social login if a local account exists
            if (user.AuthProvider == AuthProvider.Local)
            {
                throw UserErrors.EmailAlreadyExists(command.Email);
            }

            // Update username if a new one is provided and it's different
            if (!string.IsNullOrWhiteSpace(command.UserName) && user.UserName != command.UserName)
            {
                // Check if another user already takes the new username
                bool usernameExists = await userRepository.ExistsByUserNameAsync(command.UserName, cancellationToken);
                if (usernameExists)
                {
                    throw UserErrors.UsernameAlreadyExists(command.UserName);
                }

                user.UpdateUserName(command.UserName);
            }
        }
        catch (NotFoundException)
        {
            // User doesn't exist, create a new one
            user = UserEntity.CreateExternal(
                id: Guid.NewGuid(),
                userName: command.UserName,
                authProvider: provider.Value,
                email: email.Value
            );

            await userRepository.AddAsync(user, cancellationToken);
            await userRepository.AssignVisitorRoleAsync(user.Id, cancellationToken);
            await userRepository.SaveChangesAsync(cancellationToken);

            // Reload user with roles and permissions after creation
            user = await userRepository.GetPublicUserWithRolesAndPermissionsAsync(
                email.Value, cancellationToken
            );
        }

        // Handle avatar update - only download if URL changed or no avatar exists
        if (!string.IsNullOrEmpty(command.Avatar))
        {
            Guid avatarFileId = await fileService.GetOrDownloadFileAsync(
                user.AvatarFileId,
                command.Avatar,
                cancellationToken
            );
            user.UpdateAvatar(avatarFileId);
        }

        user.RecordLogin();
        await userRepository.SaveChangesAsync(cancellationToken);

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
        FileEntity? avatarFile = user.AvatarFileId.HasValue
            ? await fileRepository.GetByIdAsync(user.AvatarFileId.Value, cancellationToken)
            : null;

        // Map to userDTO with avatar
        var avatarDto = avatarFile.ToFileDto();
        var userDto = user.ToUserResponseDto(roles, permissions, avatarDto);
        var authResult = new AuthenticationResult(userDto, token.Token, token.ExpiresAt);

        return new SocialLoginResult(authResult);
    }
}
