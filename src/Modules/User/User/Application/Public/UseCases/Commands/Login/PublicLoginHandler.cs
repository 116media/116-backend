using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Shared.Contracts.Application.CQRS;
using _116.User.Application.Shared.Errors;
using _116.User.Application.Shared.Mappers;
using _116.User.Application.Shared.Repositories;
using _116.User.Application.Shared.Services;
using _116.User.Domain.Entities;
using _116.User.Domain.Results;

namespace _116.User.Application.Public.UseCases.Commands.Login;

/// <summary>
/// Handles the <see cref="PublicLoginCommand"/> to authenticate public users.
/// </summary>
/// <param name="userRepository">Repository for user data access operations.</param>
/// <param name="roleRepository">Repository for role and permission data operations.</param>
/// <param name="passwordService">Service for verifying hashed passwords.</param>
/// <param name="jwtService">Service for generating JWT tokens with user claims.</param>
/// <param name="fileRepository">Repository for accessing file metadata.</param>
public class PublicLoginHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IPasswordService passwordService,
    IJwtService jwtService,
    IFileRepository fileRepository
) : ICommandHandler<PublicLoginCommand, PublicLoginResult>
{
    /// <summary>
    /// Handles the public login command by authenticating the user with email or username.
    /// </summary>
    /// <param name="command">The public login command containing credentials.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="PublicLoginResult"/> containing public user authentication information.</returns>
    /// <exception cref="NotFoundException">Thrown when no user is found with the specified credentials.</exception>
    /// <exception cref="BadRequestException">Thrown when password is invalid.</exception>
    /// <exception cref="AuthorizationException">
    /// Thrown when the user account is inactive or not verified (HTTP 403 Forbidden).
    /// </exception>
    public async Task<PublicLoginResult> Handle(PublicLoginCommand command, CancellationToken cancellationToken)
    {
        // Get user with roles/permissions without status checks
        UserEntity user = await userRepository.GetPublicUserWithRolesAndPermissionsAsync(
            command.Credentials,
            cancellationToken
        );

        // Verify password first before revealing account status
        if (!passwordService.Verify(command.Password, user.PasswordHash))
        {
            throw UserErrors.InvalidCredentials();
        }

        // Check account status after password verification
        userRepository.IsUserAccountActive(user);
        userRepository.IsUserAccountVerified(user);

        // Extract user permissions from roles (already loaded by repository)
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
            authProvider: user.AuthProvider
        );

        // Record successful login
        user.RecordLogin();
        await userRepository.SaveChangesAsync(cancellationToken);

        // Extract roles and permissions using repository
        var (roles, permissions) = roleRepository.GetUserRolesAndPermissions(user.UserRoles);

        // Fetch the avatar file if the user has one
        FileEntity? avatarFile = user.AvatarFileId.HasValue
            ? await fileRepository.GetByIdAsync(user.AvatarFileId.Value, cancellationToken)
            : null;

        // Map to userDTO with avatar
        var avatarDto = avatarFile?.ToFileDto();
        var userDto = user.ToUserResponseDto(roles, permissions, avatarDto);
        var authResult = new AuthenticationResult(userDto, token.Token, token.ExpiresAt);

        return new PublicLoginResult(authResult);
    }
}
