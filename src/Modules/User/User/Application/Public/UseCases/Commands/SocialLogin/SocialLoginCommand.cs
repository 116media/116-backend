using _116.Shared.Contracts.Application.CQRS;
using _116.User.Domain.Enums;
using _116.User.Domain.Results;

namespace _116.User.Application.Public.UseCases.Commands.SocialLogin;

/// <summary>
/// Command for social login authentication (Google, Facebook).
/// </summary>
/// <param name="Email">The user's email address from the social provider.</param>
/// <param name="UserName">The user's display name from the social provider.</param>
/// <param name="Avatar">Optional avatar URL from the social provider.</param>
/// <param name="Provider">The social authentication provider (Google or Facebook).</param>
/// <remarks>
/// This command handles authentication through external social providers.
/// Users authenticated via social login are automatically verified and marked as active.
/// If an avatar URL is provided, it will be downloaded and stored locally.
/// </remarks>
public record SocialLoginCommand(
    string Email,
    string UserName,
    string? Avatar,
    string Provider
) : ICommand<SocialLoginResult>;

/// <summary>
/// Result of the <see cref="SocialLoginCommand"/> containing authentication details.
/// </summary>
/// <param name="AuthenticationResult">The authentication result with user info and JWT token.</param>
/// <remarks>
/// Contains authentication information for social login.
/// For new users, the account is automatically verified and activated.
/// </remarks>
public record SocialLoginResult(AuthenticationResult AuthenticationResult);
