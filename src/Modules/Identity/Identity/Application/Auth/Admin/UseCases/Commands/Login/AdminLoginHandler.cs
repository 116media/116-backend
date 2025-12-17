using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Identity.Application.Shared.Errors;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Application.Shared.Services;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Results;
using _116.Identity.Domain.ValueObjects;
using _116.Shared.Application.Exceptions;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Auth.Admin.UseCases.Commands.Login;

/// <summary>
/// Handles the <see cref="AdminLoginCommand"/> to authenticate admin users.
/// </summary>
public class AdminLoginHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IFileRepository fileRepository,
    IPasswordService passwordService,
    IJwtService jwtService,
    IIdentityUnitOfWork unitOfWork
) : ICommandHandler<AdminLoginCommand, AdminLoginResult>
{
    /// <summary>
    /// Handles the admin login command by authenticating the user and validating admin privileges.
    /// </summary>
    public async Task<AdminLoginResult> Handle(AdminLoginCommand command, CancellationToken cancellationToken)
    {
        // Normalize email using value object
        var email = new Email(command.Email);

        // Get admin user with all necessary data in one call
        UserEntity? user = await userRepository.GetUserWithRolesAndPermissionsByEmailOrThrow(
            email,
            cancellationToken
        );

        // Verify password first before revealing account status
        if (!passwordService.Verify(command.Password, user!.PasswordHash))
        {
            throw UserErrors.InvalidCredentials();
        }

        // Validate user can login
        user.ValidateCanLogin();

        // Verify admin has admin role
        userRepository.IsUserAdmin(user);

        // Extract user permissions from roles (already loaded by repository)
        List<RolePermissionEntity> userPermissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .ToList();

        // Generate JWT token with admin claims
        JwtGenerationResult token = jwtService.GenerateToken(
            userId: user.Id,
            email: user.Email!,
            userName: user.UserName,
            userRoles: user.UserRoles,
            userPermissions: userPermissions,
            isVerified: user.IsVerified,
            isActive: user.IsActive,
            authProvider: user.AuthProvider
        );

        // TODO: Create SessionEntity with refresh token when implementing session management
        // For now, login works with access token only
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
