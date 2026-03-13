using _116.Identity.Application.Shared.Errors;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.RestorePermission;

/// <summary>
/// Handles the <see cref="AdminRestorePermissionCommand" /> to restore a soft-deleted permission.
/// </summary>
/// <param name="permissionRepository">Repository for permission data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminRestorePermissionHandler(
    IPermissionRepository permissionRepository,
    IIdentityUnitOfWork unitOfWork,
    IMapper mapper
) : ICommandHandler<AdminRestorePermissionCommand, AdminRestorePermissionResult>
{
    /// <summary>
    /// Handles the permission restore command.
    /// </summary>
    /// <param name="command">The command containing the permission ID to restore.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="AdminRestorePermissionResult" /> containing the restored permission.</returns>
    public async Task<AdminRestorePermissionResult> Handle(
        AdminRestorePermissionCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid permissionId = Guid.Parse(input: command.PermissionId);

        PermissionEntity? permission = await permissionRepository.GetPermissionByIdOrThrowAsync(
            permissionId: permissionId,
            cancellationToken: cancellationToken
        );

        bool wasRestored = permission!.Restore();

        if (!wasRestored)
        {
            throw UserErrors.PermissionNotDeleted();
        }

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        var permissionDto = permission.ToPermissionDto(mapper);
        return new AdminRestorePermissionResult(Permission: permissionDto);
    }
}
