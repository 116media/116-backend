using _116.Identity.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.RestorePermission;

/// <summary>
/// Command for restoring a soft-deleted permission.
/// </summary>
/// <param name="PermissionId">The unique identifier of the permission to restore.</param>
public record AdminRestorePermissionCommand(string PermissionId) : ICommand<AdminRestorePermissionResult>;

/// <summary>
/// Result of the <see cref="AdminRestorePermissionCommand" /> containing the restored permission details.
/// </summary>
/// <param name="Permission">The restored permission information.</param>
public record AdminRestorePermissionResult(PermissionDto Permission);
