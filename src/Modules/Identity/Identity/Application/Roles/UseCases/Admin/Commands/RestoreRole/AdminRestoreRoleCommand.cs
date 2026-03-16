using _116.Identity.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.RestoreRole;

/// <summary>
/// Command for restoring a soft-deleted role.
/// </summary>
/// <param name="RoleId">The unique identifier of the role to restore.</param>
public record AdminRestoreRoleCommand(string RoleId) : ICommand<AdminRestoreRoleResult>;

/// <summary>
/// Result of the <see cref="AdminRestoreRoleCommand" /> containing the restored role details.
/// </summary>
/// <param name="Role">The restored role information.</param>
public record AdminRestoreRoleResult(RoleDto Role);
