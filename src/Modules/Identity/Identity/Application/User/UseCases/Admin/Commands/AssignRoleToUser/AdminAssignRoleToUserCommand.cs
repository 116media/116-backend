using _116.Identity.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.User.UseCases.Admin.Commands.AssignRoleToUser;

/// <summary>
/// Command for assigning a role to a user.
/// </summary>
/// <param name="UserId">The unique identifier of the user.</param>
/// <param name="RoleId">The unique identifier of the role to assign.</param>
public record AdminAssignRoleToUserCommand(string UserId, Guid RoleId) : ICommand<AdminAssignRoleToUserResult>;

/// <summary>
/// Result of the <see cref="AdminAssignRoleToUserCommand" /> containing the user's updated roles.
/// </summary>
/// <param name="Roles">The list of roles assigned to the user.</param>
public record AdminAssignRoleToUserResult(IReadOnlyCollection<RoleDto> Roles);
