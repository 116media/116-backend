using _116.Identity.Application.Adapters.SocialAuth;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.SocialLogin.Contracts;

/// <summary>
/// Contains authenticated social user data and associated permissions.
/// </summary>
public record PublicSocialLoginAuthData(UserEntity User, List<RolePermissionEntity> UserPermissions);

/// <summary>
/// Factory for handling social authentication logic.
/// </summary>
public interface IPublicSocialLoginAuthFactory
{
    /// <summary>
    /// Authenticates or creates a user from a verified social provider payload.
    /// </summary>
    /// <param name="payload">The verified identity asserted by the provider token.</param>
    /// <param name="provider">The social authentication provider.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Authentication data containing the user and its permissions.</returns>
    Task<PublicSocialLoginAuthData> AuthenticateOrCreateAsync(
        SocialTokenPayload payload,
        EnumAuthProvider provider,
        CancellationToken cancellationToken
    );
}
