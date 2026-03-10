using _116.Identity.Application.Shared.Errors;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.SoftDeletePermission;

/// <summary>
/// Handles the <see cref="AdminSoftDeletePermissionCommand" /> to soft delete a permission.
/// </summary>
/// <param name="permissionRepository">Repository for permission data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminSoftDeletePermissionHandler(
    IPermissionRepository permissionRepository,
    IIdentityUnitOfWork unitOfWork,
    IMapper mapper
) : ICommandHandler<AdminSoftDeletePermissionCommand, AdminSoftDeletePermissionResult>
{
    /// <summary>
    /// Handles the permission soft delete command.
    /// </summary>
    /// <param name="command">The command containing the permission ID to soft delete.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="AdminSoftDeletePermissionResult" /> containing the soft deleted permission.</returns>
    public async Task<AdminSoftDeletePermissionResult> Handle(
        AdminSoftDeletePermissionCommand command,
        CancellationToken cancellationToken
    )
    {
        PermissionEntity? permission = await permissionRepository.GetPermissionByIdOrThrowAsync(
            permissionId: command.PermissionId,
            cancellationToken: cancellationToken
        );

        bool wasSoftDeleted = permission!.SoftDelete();

        if (!wasSoftDeleted)
        {
            throw UserErrors.PermissionAlreadyDeleted();
        }

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        var permissionDto = permission.ToPermissionDto(mapper);
        return new AdminSoftDeletePermissionResult(Permission: permissionDto, IsSuccess: true);
    }
}
