using _116.Identity.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.SoftDeleteRole;

/// <summary>
/// Command for soft deleting a role.
/// </summary>
/// <param name="RoleId">The unique identifier of the role to soft-delete.</param>
public record AdminSoftDeleteRoleCommand(Guid RoleId) : ICommand<AdminSoftDeleteRoleResult>;

/// <summary>
/// Result of the <see cref="AdminSoftDeleteRoleCommand" /> containing the soft deleted role details.
/// </summary>
/// <param name="Role">The soft deleted role information.</param>
/// <param name="IsSuccess">Indicates whether the role was successfully soft deleted.</param>
public record AdminSoftDeleteRoleResult(RoleDto Role, bool IsSuccess);
