using _116.Shared.Contracts.Application.CQRS;

namespace _116.Auth.Application.Admin.UseCases.Commands.SignOut;

/// <summary>
/// Command for signing out an admin user and marking them as logged out.
/// </summary>
/// <param name="UserId">The unique identifier of the admin user to sign out.</param>
/// <remarks>
/// This command updates the admin user's login status to indicate they are no longer active.
/// User ID is extracted from JWT token at the endpoint level.
/// </remarks>
public record AdminSignOutCommand(
    Guid UserId
) : ICommand<AdminSignOutResult>;

/// <summary>
/// Result of the <see cref="AdminSignOutCommand"/> containing sign-out status.
/// </summary>
/// <param name="IsSuccess">Indicates if the sign-out operation was successful.</param>
/// <remarks>
/// Simple result indicating successful logout operation.
/// </remarks>
public record AdminSignOutResult(
    bool IsSuccess
);
