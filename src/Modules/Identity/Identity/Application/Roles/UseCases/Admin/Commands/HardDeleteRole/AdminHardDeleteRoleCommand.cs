using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.HardDeleteRole;

/// <summary>
/// Command for permanently deleting a role.
/// </summary>
/// <param name="RoleId">The unique identifier of the role to permanently delete.</param>
public record AdminHardDeleteRoleCommand(string RoleId) : ICommand<AdminHardDeleteRoleResult>;

/// <summary>
/// Result of the <see cref="AdminHardDeleteRoleCommand" />.
/// </summary>
/// <param name="IsSuccess">Indicates whether the role was successfully deleted.</param>
public record AdminHardDeleteRoleResult(bool IsSuccess);
