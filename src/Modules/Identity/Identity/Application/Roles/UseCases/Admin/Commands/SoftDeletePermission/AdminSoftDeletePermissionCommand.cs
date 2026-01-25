using _116.Identity.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.SoftDeletePermission;

/// <summary>
/// Command for soft deleting a permission.
/// </summary>
/// <param name="PermissionId">The unique identifier of the permission to soft-delete.</param>
public record AdminSoftDeletePermissionCommand(Guid PermissionId) : ICommand<AdminSoftDeletePermissionResult>;

/// <summary>
/// Result of the <see cref="AdminSoftDeletePermissionCommand" /> containing the soft deleted permission details.
/// </summary>
/// <param name="Permission">The soft-deleted permission information.</param>
public record AdminSoftDeletePermissionResult(PermissionDto Permission);
