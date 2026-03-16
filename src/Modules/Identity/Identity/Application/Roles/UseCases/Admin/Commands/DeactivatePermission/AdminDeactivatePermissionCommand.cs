using _116.Identity.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.DeactivatePermission;

/// <summary>
/// Command for deactivating a permission.
/// </summary>
/// <param name="PermissionId">The unique identifier of the permission to deactivate.</param>
public record AdminDeactivatePermissionCommand(string PermissionId) : ICommand<AdminDeactivatePermissionResult>;

/// <summary>
/// Result of the <see cref="AdminDeactivatePermissionCommand" /> containing the deactivated permission details.
/// </summary>
/// <param name="Permission">The deactivated permission information.</param>
public record AdminDeactivatePermissionResult(PermissionDto Permission);
