using _116.Shared.Contracts.Application.CQRS;

namespace _116.User.Application.Public.UseCases.Commands.SignOut;

/// <summary>
/// Command for signing out a user and marking them as logged out.
/// </summary>
/// <param name="UserId">The unique identifier of the user to sign out.</param>
/// <remarks>
/// This command updates the user's login status to indicate they are no longer active.
/// User ID is extracted from JWT token at the endpoint level.
/// </remarks>
public record SignOutCommand(
    Guid UserId
) : ICommand<SignOutResult>;

/// <summary>
/// Result of the <see cref="SignOutCommand"/> containing sign-out status.
/// </summary>
/// <param name="IsSuccess">Indicates if the sign-out operation was successful.</param>
/// <remarks>
/// Simple result indicating successful logout operation.
/// </remarks>
public record SignOutResult(
    bool IsSuccess
);
