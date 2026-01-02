using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Domain.Entities;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.SocialLogin.Contracts;

/// <summary>
/// Contains authenticated social user data and associated roles/permissions.
/// </summary>
public record PublicSocialLoginAuthData(
    UserEntity User,
    List<RolePermissionEntity> UserPermissions,
    IReadOnlyCollection<RoleDto> Roles,
    IReadOnlyCollection<PermissionDto> Permissions
);

/// <summary>
/// Factory for handling social authentication logic.
/// </summary>
public interface IPublicSocialLoginAuthFactory
{
    /// <summary>
    /// Authenticates or creates a user via social provider.
    /// </summary>
    /// <param name="email">The user's email from social provider.</param>
    /// <param name="userName">The user's username from social provider.</param>
    /// <param name="provider">The social authentication provider.</param>
    /// <param name="avatarUrl">The avatar URL from social provider.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Authentication data containing user, permissions, roles, and permission names.</returns>
    Task<PublicSocialLoginAuthData> AuthenticateOrCreateAsync(
        string email,
        string userName,
        string provider,
        string? avatarUrl,
        CancellationToken cancellationToken
    );
}
