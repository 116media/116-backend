using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.Login.Contracts;
using _116.Identity.Application.Shared.Errors;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.ValueObjects;

namespace _116.Identity.Application.Auth.UseCases.Admin.Commands.Login;

/// <summary>
/// Factory implementation for handling admin user authentication logic in the login flow.
/// </summary>
/// <param name="authRepository">Repository for user data access operations.</param>
/// <param name="passwordService">Service for verifying hashed passwords.</param>
/// <param name="roleRepository">Repository for role and permission data operations.</param>
public class AdminLoginAuthFactory(
    IAuthRepository authRepository,
    IPasswordService passwordService,
    IRoleRepository roleRepository
) : IAdminLoginAuthFactory
{
    /// <summary>
    /// Authenticates an admin user with their email and password.
    /// </summary>
    public async Task<AdminLoginAuthData> AuthenticateAsync(
        string email,
        string password,
        CancellationToken cancellationToken
    )
    {
        UserEntity? user = await authRepository.GetUserWithRolesAndPermissionsByEmailOrThrow(
            new Email(value: email),
            cancellationToken: cancellationToken
        );

        // Verify password first before revealing account status
        if (!passwordService.Verify(password: password, hash: user!.PasswordHash))
        {
            throw UserErrors.InvalidCredentials();
        }

        user.ValidateCanLogin();
        authRepository.IsUserAdmin(user: user);

        List<RolePermissionEntity> userPermissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .ToList();

        var (roles, permissions) = roleRepository.GetUserRolesAndPermissions(userRoles: user.UserRoles);

        return new AdminLoginAuthData(
            User: user,
            UserPermissions: userPermissions,
            Roles: roles,
            Permissions: permissions
        );
    }
}
