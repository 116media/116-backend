using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.User.UseCases.Admin.Commands.DeactivateUser;

/// <summary>
/// Command used to deactivate a user account (admin only).
/// </summary>
/// <param name="UserId">The ID of the user to deactivate (as string, validated and parsed in handler).</param>
public record AdminDeactivateUserCommand(string UserId) : ICommand<AdminDeactivateUserResult>;

/// <summary>
/// The result of executing an <see cref="AdminDeactivateUserCommand" />.
/// </summary>
/// <param name="IsSuccess">Indicates whether the account is deactivated after the call.</param>
public record AdminDeactivateUserResult(bool IsSuccess);
