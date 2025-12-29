using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Domain.Entities;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.Login.Contracts;

/// <summary>
/// Contains authenticated user data and associated roles/permissions.
/// </summary>
public record PublicLoginAuthData(
    UserEntity User,
    List<RolePermissionEntity> UserPermissions,
    IReadOnlyCollection<RoleDto> Roles,
    IReadOnlyCollection<PermissionDto> Permissions
);

/// <summary>
/// Factory for handling user authentication logic in the login flow.
/// </summary>
public interface IPublicLoginAuthFactory
{
    /// <summary>
    /// Authenticates a user with their credentials and password.
    /// </summary>
    /// <param name="credentials">The user's email or username.</param>
    /// <param name="password">The user's password.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Authentication data containing user, permissions, roles, and permission names.</returns>
    Task<PublicLoginAuthData> AuthenticateAsync(
        string credentials,
        string password,
        CancellationToken cancellationToken
    );
}
