using _116.Identity.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.UpdatePermission;

/// <summary>
/// Command for updating an existing permission.
/// </summary>
/// <param name="PermissionId">The unique identifier of the permission to update.</param>
/// <param name="Resource">The new resource name (optional).</param>
/// <param name="Action">The new action name (optional).</param>
/// <param name="Description">The new description (optional).</param>
public record AdminUpdatePermissionCommand(string PermissionId, string? Resource, string? Action, string? Description)
    : ICommand<AdminUpdatePermissionResult>;

/// <summary>
/// Result of the <see cref="AdminUpdatePermissionCommand" /> containing the updated permission details.
/// </summary>
/// <param name="Permission">The updated permission information.</param>
public record AdminUpdatePermissionResult(PermissionDto Permission);
