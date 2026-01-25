using _116.Identity.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.RemovePermissionFromRole;

/// <summary>
/// Command for removing a permission from a role.
/// </summary>
/// <param name="RoleId">The unique identifier of the role.</param>
/// <param name="PermissionId">The unique identifier of the permission to remove.</param>
public record AdminRemovePermissionFromRoleCommand(Guid RoleId, Guid PermissionId)
    : ICommand<AdminRemovePermissionFromRoleResult>;

/// <summary>
/// Result of the <see cref="AdminRemovePermissionFromRoleCommand" /> containing the role with updated permissions.
/// </summary>
/// <param name="Role">The role information with permissions.</param>
public record AdminRemovePermissionFromRoleResult(RoleWithPermissionsDto Role);
