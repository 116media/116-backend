using _116.Identity.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.CreatePermission;

/// <summary>
/// Command for creating a new permission.
/// </summary>
/// <param name="Resource">The resource name (e.g., "user", "article").</param>
/// <param name="Action">The action name (e.g., "read", "create").</param>
/// <param name="Description">A description of the permission's purpose.</param>
public record AdminCreatePermissionCommand(string Resource, string Action, string Description)
    : ICommand<AdminCreatePermissionResult>;

/// <summary>
/// Result of the <see cref="AdminCreatePermissionCommand" /> containing the created permission details.
/// </summary>
/// <param name="Permission">The created permission information.</param>
public record AdminCreatePermissionResult(PermissionDto Permission);
