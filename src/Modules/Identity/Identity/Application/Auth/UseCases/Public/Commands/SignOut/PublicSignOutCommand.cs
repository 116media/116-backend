using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.SignOut;

/// <summary>
/// Command for signing out a user from the current device/session.
/// </summary>
/// <param name="UserId">The unique identifier of the user to sign out.</param>
/// <param name="RefreshToken">The refresh token identifying the session to invalidate.</param>
/// <remarks>
/// This command invalidates the specific session associated with the refresh token.
/// User ID is extracted from JWT token at the endpoint level.
/// </remarks>
public record PublicSignOutCommand(
    Guid UserId,
    string RefreshToken
) : ICommand<PublicSignOutResult>;

/// <summary>
/// Result of the <see cref="PublicSignOutCommand" /> containing sign-out status.
/// </summary>
/// <param name="IsSuccess">Indicates if the sign-out operation was successful.</param>
/// <remarks>
/// Simple result indicating successful logout operation.
/// </remarks>
public record PublicSignOutResult(
    bool IsSuccess
);
