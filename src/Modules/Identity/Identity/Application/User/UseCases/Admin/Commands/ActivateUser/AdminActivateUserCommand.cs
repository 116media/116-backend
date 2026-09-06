using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.User.UseCases.Admin.Commands.ActivateUser;

/// <summary>
/// Command used to reactivate a user account (admin only).
/// </summary>
/// <param name="UserId">The ID of the user to activate (as string, validated and parsed in handler).</param>
public record AdminActivateUserCommand(string UserId) : ICommand<AdminActivateUserResult>;

/// <summary>
/// The result of executing an <see cref="AdminActivateUserCommand" />.
/// </summary>
/// <param name="IsSuccess">Indicates whether the account is active after the call.</param>
public record AdminActivateUserResult(bool IsSuccess);
