using _116.Identity.Application.Shared.Errors;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.ActivatePermission;

/// <summary>
/// Handles the <see cref="AdminActivatePermissionCommand" /> to activate a permission.
/// </summary>
/// <param name="permissionRepository">Repository for permission data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminActivatePermissionHandler(
    IPermissionRepository permissionRepository,
    IIdentityUnitOfWork unitOfWork,
    IMapper mapper
) : ICommandHandler<AdminActivatePermissionCommand, AdminActivatePermissionResult>
{
    /// <summary>
    /// Handles the permission activation command.
    /// </summary>
    /// <param name="command">The command containing the permission ID to activate.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="AdminActivatePermissionResult" /> containing the activated permission.</returns>
    public async Task<AdminActivatePermissionResult> Handle(
        AdminActivatePermissionCommand command,
        CancellationToken cancellationToken
    )
    {
        PermissionEntity? permission = await permissionRepository.GetPermissionByIdOrThrowAsync(
            permissionId: command.PermissionId,
            cancellationToken: cancellationToken
        );

        bool wasActivated = permission!.Activate();

        if (!wasActivated)
        {
            throw UserErrors.PermissionAlreadyActive();
        }

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        var permissionDto = permission.ToPermissionDto(mapper);
        return new AdminActivatePermissionResult(Permission: permissionDto);
    }
}
