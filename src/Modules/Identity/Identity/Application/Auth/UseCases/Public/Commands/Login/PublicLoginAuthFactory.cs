using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Auth.UseCases.Public.Commands.Login.Contracts;
using _116.Identity.Application.Shared.Errors;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.Login;

/// <summary>
/// Factory implementation for handling user authentication logic in the login flow.
/// </summary>
/// <param name="authRepository">Repository for user data access operations.</param>
/// <param name="passwordService">Service for verifying hashed passwords.</param>
/// <param name="roleRepository">Repository for role and permission data operations.</param>
public class PublicLoginAuthFactory(
    IAuthRepository authRepository,
    IPasswordService passwordService,
    IRoleRepository roleRepository
) : IPublicLoginAuthFactory
{
    /// <summary>
    /// Authenticates a user with their credentials and password.
    /// </summary>
    public async Task<PublicLoginAuthData> AuthenticateAsync(
        string credentials,
        string password,
        CancellationToken cancellationToken
    )
    {
        UserEntity? user = await authRepository.GetUserWithRolesAndPermissionsByCredentialsOrThrow(
            credentials: credentials,
            cancellationToken: cancellationToken
        );

        if (!passwordService.Verify(password: password, hash: user!.PasswordHash))
        {
            throw UserErrors.InvalidCredentials();
        }

        user.ValidateCanLogin();

        List<RolePermissionEntity> userPermissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .ToList();

        var (roles, permissions) = roleRepository.GetUserRolesAndPermissions(userRoles: user.UserRoles);

        return new PublicLoginAuthData(
            User: user,
            UserPermissions: userPermissions,
            Roles: roles,
            Permissions: permissions
        );
    }
}
