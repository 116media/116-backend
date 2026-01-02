using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Session.UseCases.Admin.Commands.ForceLogoutUser;

/// <summary>
/// Command used to force logout a user from all their sessions (admin only).
/// </summary>
/// <param name="UserId">The ID of the user to force logout (as string, validated and parsed in handler).</param>
public record AdminForceLogoutUserCommand(string UserId) : ICommand<AdminForceLogoutUserResult>;

/// <summary>
/// The result of executing an <see cref="AdminForceLogoutUserCommand" />.
/// </summary>
/// <param name="IsSuccess">Indicates whether the user was successfully logged out from all devices.</param>
public record AdminForceLogoutUserResult(bool IsSuccess);
