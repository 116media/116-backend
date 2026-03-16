using _116.Identity.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.UpdateRole;

/// <summary>
/// Command for updating an existing role.
/// </summary>
/// <param name="RoleId">The unique identifier of the role to update.</param>
/// <param name="Name">The new name for the role (optional).</param>
/// <param name="Description">The new description for the role (optional).</param>
public record AdminUpdateRoleCommand(string RoleId, string? Name, string? Description)
    : ICommand<AdminUpdateRoleResult>;

/// <summary>
/// Result of the <see cref="AdminUpdateRoleCommand" /> containing the updated role details.
/// </summary>
/// <param name="Role">The updated role information.</param>
public record AdminUpdateRoleResult(RoleDto Role);
