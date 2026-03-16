using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.HardDeletePermission;

/// <summary>
/// Handles the <see cref="AdminHardDeletePermissionCommand" /> to permanently delete a permission.
/// </summary>
/// <param name="permissionRepository">Repository for permission data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminHardDeletePermissionHandler(
    IPermissionRepository permissionRepository,
    IIdentityUnitOfWork unitOfWork
) : ICommandHandler<AdminHardDeletePermissionCommand, AdminHardDeletePermissionResult>
{
    /// <summary>
    /// Handles the permission hard delete command.
    /// </summary>
    /// <param name="command">The command containing the permission ID to permanently delete.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="AdminHardDeletePermissionResult" /> indicating success.</returns>
    public async Task<AdminHardDeletePermissionResult> Handle(
        AdminHardDeletePermissionCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid permissionId = Guid.Parse(input: command.PermissionId);

        PermissionEntity? permission = await permissionRepository.GetPermissionByIdOrThrowAsync(
            permissionId: permissionId,
            cancellationToken: cancellationToken
        );

        permissionRepository.Delete(entity: permission!);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminHardDeletePermissionResult(IsSuccess: true);
    }
}
