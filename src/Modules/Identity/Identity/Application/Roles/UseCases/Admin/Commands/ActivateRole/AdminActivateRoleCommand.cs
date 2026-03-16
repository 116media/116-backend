using _116.Identity.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.ActivateRole;

/// <summary>
/// Command for activating a role.
/// </summary>
/// <param name="RoleId">The unique identifier of the role to activate.</param>
public record AdminActivateRoleCommand(string RoleId) : ICommand<AdminActivateRoleResult>;

/// <summary>
/// Result of the <see cref="AdminActivateRoleCommand" /> containing the activated role details.
/// </summary>
/// <param name="Role">The activated role information.</param>
public record AdminActivateRoleResult(RoleDto Role);
