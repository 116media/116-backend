using _116.Shared.Application.Exceptions;
using _116.Identity.Application.Shared.Persistence;
using _116.Shared.Contracts.Application.CQRS;
using _116.Identity.Application.Shared.Errors;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Application.Shared.Services;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Results;
using _116.Identity.Domain.ValueObjects;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;

namespace _116.Identity.Application.Admin.UseCases.Commands.Login;

/// <summary>
/// Handles the <see cref="AdminLoginCommand"/> to authenticate admin users.
/// </summary>
/// <param name="userRepository">Repository for user data access operations.</param>
/// <param name="roleRepository">Repository for role and permission data operations.</param>
/// <param name="passwordService">Service for verifying hashed passwords.</param>
/// <param name="jwtService">Service for generating JWT tokens with admin claims.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminLoginHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IFileRepository fileRepository,
    IPasswordService passwordService,
    IJwtService jwtService,
    IAuthUnitOfWork unitOfWork
) : ICommandHandler<AdminLoginCommand, AdminLoginResult>
{
    /// <summary>
    /// Handles the admin login command by authenticating the user and validating admin privileges.
    /// </summary>
    /// <param name="command">The admin login command containing credentials.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>An <see cref="AdminLoginResult"/> containing admin authentication information.</returns>
    /// <exception cref="NotFoundException">Thrown when no user is found with the specified email.</exception>
    /// <exception cref="BadRequestException">Thrown when password is invalid.</exception>
    /// <exception cref="AuthorizationException">
    /// Thrown when the user account is inactive (HTTP 403 Forbidden).
    /// </exception>
    /// <exception cref="AuthenticationException">
    /// Thrown when the user lacks administrative privileges (HTTP 401 Unauthorized).
    /// </exception>
    public async Task<AdminLoginResult> Handle(AdminLoginCommand command, CancellationToken cancellationToken)
    {
        // Normalize email using value object
        var email = new Email(command.Email);

        // Get admin user with all necessary data in one call
        UserEntity? user = await userRepository.GetUserWithRolesAndPermissionsByEmailOrThrow(
            email,
            cancellationToken
        );

        // Validate admin account status
        userRepository.IsUserAccountActive(user!);
        userRepository.IsUserAdmin(user!);

        // Verify password
        if (!passwordService.Verify(command.Password, user!.PasswordHash))
        {
            throw UserErrors.InvalidCredentials();
        }

        // Extract user permissions from roles (already loaded by repository)
        List<RolePermissionEntity> userPermissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .ToList();

        // Generate JWT token with admin claims
        JwtGenerationResult token = jwtService.GenerateToken(
            userId: user.Id,
            email: email.Value,
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
        await unitOfWork.CommitAsync(cancellationToken);

        // Extract roles and permissions using repository
        var (roles, permissions) = roleRepository.GetUserRolesAndPermissions(user.UserRoles);

        // Fetch the avatar file if the user has one
        FileEntity? avatarFile = await fileRepository.GetAvatarFileAsync(user.AvatarFileId, cancellationToken);

        // Map to userDTO with avatar
        var avatarDto = avatarFile?.ToFileDto();
        var userDto = user.ToUserResponseDto(roles, permissions, avatarDto);
        var authResult = new AuthenticationResult(userDto, token.Token, token.ExpiresAt);

        return new AdminLoginResult(authResult);
    }
}
