using _116.Identity.Application.Shared.Errors;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.DeactivatePermission;

/// <summary>
/// Handles the <see cref="AdminDeactivatePermissionCommand" /> to deactivate a permission.
/// </summary>
/// <param name="permissionRepository">Repository for permission data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminDeactivatePermissionHandler(
    IPermissionRepository permissionRepository,
    IIdentityUnitOfWork unitOfWork
) : ICommandHandler<AdminDeactivatePermissionCommand, AdminDeactivatePermissionResult>
{
    /// <summary>
    /// Handles the permission deactivation command.
    /// </summary>
    /// <param name="command">The command containing the permission ID to deactivate.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="AdminDeactivatePermissionResult" /> containing the deactivated permission.</returns>
    public async Task<AdminDeactivatePermissionResult> Handle(
        AdminDeactivatePermissionCommand command,
        CancellationToken cancellationToken
    )
    {
        PermissionEntity? permission = await permissionRepository.GetPermissionByIdOrThrowAsync(
            permissionId: command.PermissionId,
            cancellationToken: cancellationToken
        );

        bool wasDeactivated = permission!.Deactivate();

        if (!wasDeactivated)
        {
            throw UserErrors.PermissionAlreadyInactive();
        }

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        var permissionDto = permission.ToPermissionDto();
        return new AdminDeactivatePermissionResult(Permission: permissionDto);
    }
}
