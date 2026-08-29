using _116.Identity.Domain.Results;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.SocialLogin;

/// <summary>
/// Command for social login authentication (Google, Facebook).
/// </summary>
/// <param name="Provider">The social authentication provider (Google or Facebook).</param>
/// <param name="IdToken">The provider-issued token to verify.</param>
/// <remarks>
/// This command handles authentication through external social providers. The provider token is
/// verified server-side; the verified identity (email, name, avatar) is resolved from it, and new
/// users are automatically verified and marked as active.
/// </remarks>
public record PublicSocialLoginCommand(string Provider, string IdToken) : ICommand<PublicSocialLoginResult>;

/// <summary>
/// Result of the <see cref="PublicSocialLoginCommand" /> containing authentication details.
/// </summary>
/// <param name="AuthenticationResult">The authentication result with user info and JWT token.</param>
/// <remarks>
/// Contains authentication information for social login.
/// For new users, the account is automatically verified and activated.
/// </remarks>
public record PublicSocialLoginResult(AuthenticationResult AuthenticationResult);
