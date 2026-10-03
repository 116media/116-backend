using _116.BuildingBlocks.Application.CQRS;
using _116.Identity.Application.Roles.UseCases.Admin.Commands.SoftDeletePermission.Contracts;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Domain.Entities;
using MapsterMapper;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.SoftDeletePermission;

/// <summary>
/// Handles the <see cref="AdminSoftDeletePermissionCommand" /> to soft-delete a permission.
/// </summary>
/// <param name="softDeletePermissionService">Service loading the permission and applying the deletion.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminSoftDeletePermissionHandler(
    IAdminSoftDeletePermissionService softDeletePermissionService,
    IIdentityUnitOfWork unitOfWork,
    IMapper mapper
) : ICommandHandler<AdminSoftDeletePermissionCommand, AdminSoftDeletePermissionResult>
{
    /// <inheritdoc />
    public async Task<AdminSoftDeletePermissionResult> Handle(
        AdminSoftDeletePermissionCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid permissionId = Guid.Parse(input: command.PermissionId);

        PermissionEntity permission = await softDeletePermissionService.SoftDeleteAsync(
            permissionId: permissionId,
            cancellationToken: cancellationToken
        );

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        var permissionDto = permission.ToPermissionDto(mapper);
        return new AdminSoftDeletePermissionResult(Permission: permissionDto, IsSuccess: true);
    }
}
