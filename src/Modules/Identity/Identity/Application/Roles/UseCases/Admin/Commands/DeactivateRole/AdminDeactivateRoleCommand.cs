using _116.Identity.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.DeactivateRole;

/// <summary>
/// Command for deactivating a role.
/// </summary>
/// <param name="RoleId">The unique identifier of the role to deactivate.</param>
public record AdminDeactivateRoleCommand(string RoleId) : ICommand<AdminDeactivateRoleResult>;

/// <summary>
/// Result of the <see cref="AdminDeactivateRoleCommand" /> containing the deactivated role details.
/// </summary>
/// <param name="Role">The deactivated role information.</param>
public record AdminDeactivateRoleResult(RoleDto Role);
