using _116.Identity.Domain.Entities;

namespace _116.Identity.Application.Auth.UseCases.Admin.Commands.Login.Contracts;

/// <summary>
/// Contains authenticated admin user data and associated permissions.
/// </summary>
public record AdminLoginAuthData(UserEntity User, List<RolePermissionEntity> UserPermissions);

/// <summary>
/// Factory for handling admin user authentication logic in the login flow.
/// </summary>
public interface IAdminLoginAuthFactory
{
    /// <summary>
    /// Authenticates an admin user with their email and password.
    /// </summary>
    /// <param name="email">The admin user's email.</param>
    /// <param name="password">The admin user's password.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Authentication data containing user, permissions, roles, and permission names.</returns>
    Task<AdminLoginAuthData> AuthenticateAsync(string email, string password, CancellationToken cancellationToken);
}
