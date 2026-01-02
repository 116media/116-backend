using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Auth.UseCases.Admin.Commands.SignOut;

/// <summary>
/// Command for signing out an admin user from the current device/session.
/// </summary>
/// <param name="UserId">The unique identifier of the admin user to sign out.</param>
/// <param name="RefreshToken">The refresh token identifying the session to invalidate.</param>
/// <remarks>
/// This command invalidates the specific session associated with the refresh token.
/// User ID is extracted from JWT token at the endpoint level.
/// </remarks>
public record AdminSignOutCommand(
    Guid UserId,
    string RefreshToken
) : ICommand<AdminSignOutResult>;

/// <summary>
/// Result of the <see cref="AdminSignOutCommand" /> containing sign-out status.
/// </summary>
/// <param name="IsSuccess">Indicates if the sign-out operation was successful.</param>
/// <remarks>
/// Simple result indicating successful logout operation.
/// </remarks>
public record AdminSignOutResult(
    bool IsSuccess
);
