using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.SocialLogin.Contracts;

/// <summary>
/// Contains the authenticated social user and the permissions the session is issued with.
/// </summary>
/// <param name="User">The user aggregate with its roles and permissions.</param>
/// <param name="UserPermissions">The permissions granted through the user's roles.</param>
public record PublicSocialLoginAuthData(UserEntity User, List<RolePermissionEntity> UserPermissions);

/// <summary>
/// Authenticates a social login: verifies the provider token, refuses an unverified provider
/// email, and resolves or creates the external user with the provider's avatar.
/// </summary>
public interface IPublicSocialLoginAuthService
{
    /// <summary>
    /// Verifies the provider token and authenticates or creates the user it identifies.
    /// </summary>
    /// <param name="provider">The social provider that issued the token.</param>
    /// <param name="idToken">The raw provider token.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The authenticated user and the permissions the session is issued with.</returns>
    Task<PublicSocialLoginAuthData> AuthenticateAsync(
        EnumAuthProvider provider,
        string idToken,
        CancellationToken cancellationToken
    );
}
