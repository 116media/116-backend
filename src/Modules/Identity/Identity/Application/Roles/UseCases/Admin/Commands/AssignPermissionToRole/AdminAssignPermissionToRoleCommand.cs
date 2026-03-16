using _116.Identity.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.AssignPermissionToRole;

/// <summary>
/// Command for assigning a permission to a role.
/// </summary>
/// <param name="RoleId">The unique identifier of the role.</param>
/// <param name="PermissionId">The unique identifier of the permission to assign.</param>
public record AdminAssignPermissionToRoleCommand(string RoleId, Guid PermissionId)
    : ICommand<AdminAssignPermissionToRoleResult>;

/// <summary>
/// Result of the <see cref="AdminAssignPermissionToRoleCommand" /> containing the role with updated permissions.
/// </summary>
/// <param name="Role">The role information with permissions.</param>
public record AdminAssignPermissionToRoleResult(RoleWithPermissionsDto Role);
