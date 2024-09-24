using _116.Shared.Application.Exceptions;
using _116.Shared.Contracts.Application.CQRS;
using _116.Auth.Application.Shared.Errors;
using _116.Auth.Application.Shared.Mappers;
using _116.Auth.Application.Shared.Repositories;
using _116.Auth.Application.Shared.Services;
using _116.Auth.Domain.Entities;
using _116.Auth.Domain.Results;
using _116.Auth.Domain.ValueObjects;

namespace _116.Auth.Application.Admin.UseCases.Commands.Login;

/// <summary>
/// Handles the <see cref="AdminLoginCommand"/> to authenticate admin users.
/// </summary>
/// <param name="userRepository">Repository for user data access operations.</param>
/// <param name="roleRepository">Repository for role and permission data operations.</param>
/// <param name="passwordService">Service for verifying hashed passwords.</param>
/// <param name="jwtService">Service for generating JWT tokens with admin claims.</param>
public class AdminLoginHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IPasswordService passwordService,
    IJwtService jwtService
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
        UserEntity user = await userRepository.GetActiveAdminUserWithRolesAndPermissionsAsync(
            email,
            cancellationToken
        );

        // Verify password
        if (!passwordService.Verify(command.Password, user.PasswordHash))
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
        await userRepository.SaveChangesAsync(cancellationToken);

        // Extract roles and permissions using repository
        var (roles, permissions) = roleRepository.GetUserRolesAndPermissions(user.UserRoles);

        // Map to userDTO
        var userDto = user.ToUserResponseDto(roles, permissions);
        var authResult = new AuthenticationResult(userDto, token.Token, token.ExpiresAt);

        return new AdminLoginResult(authResult);
    }
}
