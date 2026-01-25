using _116.Identity.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.BulkUpdateRolePermissions;

/// <summary>
/// Command for bulk updating permissions of a role.
/// </summary>
/// <param name="RoleId">The unique identifier of the role.</param>
/// <param name="PermissionIds">The list of permission IDs to assign to the role.</param>
public record AdminBulkUpdateRolePermissionsCommand(Guid RoleId, List<Guid> PermissionIds)
    : ICommand<AdminBulkUpdateRolePermissionsResult>;

/// <summary>
/// Result of the <see cref="AdminBulkUpdateRolePermissionsCommand" /> containing the role with updated permissions.
/// </summary>
/// <param name="Role">The role information with permissions.</param>
public record AdminBulkUpdateRolePermissionsResult(RoleWithPermissionsDto Role);
