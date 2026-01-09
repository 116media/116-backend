using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Auth.UseCases.Admin.Commands.SignOutFromAllDevices;

/// <summary>
/// Command for signing out an admin user from all devices/sessions.
/// </summary>
/// <param name="UserId">The unique identifier of the admin user to sign out from all devices.</param>
/// <remarks>
/// This command invalidates all active sessions for the admin user across all devices.
/// Commonly used after password changes or when admin suspects account compromise.
/// User ID is extracted from JWT token at the endpoint level.
/// </remarks>
public record AdminSignOutFromAllDevicesCommand(Guid UserId) : ICommand<AdminSignOutFromAllDevicesResult>;

/// <summary>
/// Result of the <see cref="AdminSignOutFromAllDevicesCommand" /> containing sign-out status.
/// </summary>
/// <param name="IsSuccess">Indicates if the sign-out operation was successful.</param>
/// <remarks>
/// Simple result indicating successful logout from all devices.
/// </remarks>
public record AdminSignOutFromAllDevicesResult(bool IsSuccess);
