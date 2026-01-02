using _116.Identity.Domain.Results;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Session.UseCases.Public.Commands.RefreshToken;

/// <summary>
/// Command used to refresh an expired access token using a valid refresh token.
/// </summary>
/// <param name="RefreshToken">The refresh token to validate and rotate.</param>
/// <remarks>
/// This command implements token rotation for security.
/// The old refresh token is invalidated and a new one is generated.
/// </remarks>
public record PublicRefreshTokenCommand(string RefreshToken) : ICommand<PublicRefreshTokenResult>;

/// <summary>
/// The result of executing a <see cref="PublicRefreshTokenCommand" />.
/// </summary>
/// <param name="AuthenticationResult">The authentication result with new tokens.</param>
public record PublicRefreshTokenResult(AuthenticationResult AuthenticationResult);
