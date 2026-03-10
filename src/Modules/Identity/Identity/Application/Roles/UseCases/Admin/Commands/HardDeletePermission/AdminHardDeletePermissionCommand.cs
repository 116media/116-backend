using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.HardDeletePermission;

/// <summary>
/// Command for permanently deleting a permission.
/// </summary>
/// <param name="PermissionId">The unique identifier of the permission to permanently delete.</param>
public record AdminHardDeletePermissionCommand(Guid PermissionId) : ICommand<AdminHardDeletePermissionResult>;

/// <summary>
/// Result of the <see cref="AdminHardDeletePermissionCommand" />.
/// </summary>
/// <param name="IsSuccess">Indicates whether the permission was successfully deleted.</param>
public record AdminHardDeletePermissionResult(bool IsSuccess);
