using _116.Identity.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.ActivatePermission;

/// <summary>
/// Command for activating a permission.
/// </summary>
/// <param name="PermissionId">The unique identifier of the permission to activate.</param>
public record AdminActivatePermissionCommand(string PermissionId) : ICommand<AdminActivatePermissionResult>;

/// <summary>
/// Result of the <see cref="AdminActivatePermissionCommand" /> containing the activated permission details.
/// </summary>
/// <param name="Permission">The activated permission information.</param>
public record AdminActivatePermissionResult(PermissionDto Permission);
