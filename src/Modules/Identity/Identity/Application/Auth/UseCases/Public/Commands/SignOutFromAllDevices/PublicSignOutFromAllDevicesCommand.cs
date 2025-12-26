using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.SignOutFromAllDevices;

/// <summary>
/// Command for signing out a user from all devices/sessions.
/// </summary>
/// <param name="UserId">The unique identifier of the user to sign out from all devices.</param>
/// <remarks>
/// This command invalidates all active sessions for the user across all devices.
/// Commonly used after password changes or when user suspects account compromise.
/// User ID is extracted from JWT token at the endpoint level.
/// </remarks>
public record PublicSignOutFromAllDevicesCommand(
    Guid UserId
) : ICommand<PublicSignOutFromAllDevicesResult>;

/// <summary>
/// Result of the <see cref="PublicSignOutFromAllDevicesCommand" /> containing sign-out status.
/// </summary>
/// <param name="IsSuccess">Indicates if the sign-out operation was successful.</param>
/// <remarks>
/// Simple result indicating successful logout from all devices.
/// </remarks>
public record PublicSignOutFromAllDevicesResult(bool IsSuccess);
