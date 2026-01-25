using _116.Identity.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.User.UseCases.Admin.Commands.RemoveRoleFromUser;

/// <summary>
/// Command for removing a role from a user.
/// </summary>
/// <param name="UserId">The unique identifier of the user.</param>
/// <param name="RoleId">The unique identifier of the role to remove.</param>
public record AdminRemoveRoleFromUserCommand(Guid UserId, Guid RoleId) : ICommand<AdminRemoveRoleFromUserResult>;

/// <summary>
/// Result of the <see cref="AdminRemoveRoleFromUserCommand" /> containing the user's updated roles.
/// </summary>
/// <param name="Roles">The list of roles assigned to the user.</param>
public record AdminRemoveRoleFromUserResult(IReadOnlyCollection<RoleDto> Roles);
