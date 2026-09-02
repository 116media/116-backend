using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.Login.Contracts;
using _116.Identity.Application.Shared.Errors;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.ValueObjects;

namespace _116.Identity.Application.Auth.UseCases.Admin.Commands.Login;

/// <summary>
/// Factory implementation for handling admin user authentication logic in the login flow.
/// An unknown account and a wrong password produce the same error after the same work, so login
/// cannot be used to discover which addresses exist.
/// </summary>
/// <param name="authRepository">Repository for user data access operations.</param>
/// <param name="passwordService">Service for verifying hashed passwords.</param>
/// <param name="lockoutRepository">Repository holding the account's failed-login counters.</param>
/// <param name="userErrors">User domain error factory for generating domain exceptions.</param>
public class AdminLoginAuthFactory(
    IAuthRepository authRepository,
    IPasswordService passwordService,
    IAccountLockoutRepository lockoutRepository,
    UserErrors userErrors
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
        UserEntity? user = await authRepository.GetUserWithRolesAndPermissionsByEmailAsync(
            new Email(value: email),
            cancellationToken: cancellationToken
        );

        if (user is not null)
        {
            AccountLockoutState lockout = await lockoutRepository.GetAsync(
                userId: user.Id,
                cancellationToken: cancellationToken
            );

            if (lockout.LockedUntil > DateTime.UtcNow)
            {
                throw userErrors.InvalidCredentials();
            }
        }

        // Runs the full derivation even when the account is unknown, so both branches cost the same.
        bool passwordMatches = passwordService.VerifyOrDummy(password: password, hash: user?.PasswordHash);

        if (user is null || !passwordMatches)
        {
            if (user is not null)
            {
                await lockoutRepository.RegisterFailedLoginAsync(userId: user.Id, cancellationToken: cancellationToken);
            }

            throw userErrors.InvalidCredentials();
        }

        user.ValidateCanLogin(errors: userErrors);
        authRepository.IsUserAdmin(user: user);
        await lockoutRepository.ClearFailedLoginsAsync(userId: user.Id, cancellationToken: cancellationToken);

        // A hash written at the old work factor is upgraded in place. InitializePasswordHash is
        // used rather than UpdatePassword so the upgrade raises no password-changed notification.
        if (passwordService.NeedsRehash(hash: user.PasswordHash))
        {
            user.InitializePasswordHash(newPasswordHash: passwordService.Hash(password: password), errors: userErrors);
        }

        List<RolePermissionEntity> userPermissions = user.UserRoles.SelectMany(ur => ur.Role.RolePermissions).ToList();

        return new AdminLoginAuthData(User: user, UserPermissions: userPermissions);
    }
}
