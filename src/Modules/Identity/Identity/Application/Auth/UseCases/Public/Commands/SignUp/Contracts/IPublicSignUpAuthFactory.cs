using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Domain.Entities;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.SignUp.Contracts;

/// <summary>
/// Contains registered user data and associated roles/permissions.
/// </summary>
public record PublicSignUpAuthData(
    UserEntity User,
    List<RolePermissionEntity> UserPermissions,
    IReadOnlyCollection<RoleDto> Roles,
    IReadOnlyCollection<PermissionDto> Permissions
);

/// <summary>
/// Factory for handling user registration logic in the signup flow.
/// </summary>
public interface IPublicSignUpAuthFactory
{
    /// <summary>
    /// Registers a new user with email, username, and password.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <param name="userName">The user's username.</param>
    /// <param name="password">The user's password.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Signup data containing user, permissions, roles, and permission names.</returns>
    Task<PublicSignUpAuthData> RegisterAsync(
        string email,
        string userName,
        string password,
        CancellationToken cancellationToken
    );
}
