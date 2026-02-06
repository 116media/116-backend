using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.Shared.Errors;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.CreatePermission;

/// <summary>
/// Handles the <see cref="AdminCreatePermissionCommand" /> to create a new permission.
/// </summary>
/// <param name="permissionRepository">Repository for permission data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminCreatePermissionHandler(
    IPermissionRepository permissionRepository,
    IIdentityUnitOfWork unitOfWork,
    IMapper mapper
) : ICommandHandler<AdminCreatePermissionCommand, AdminCreatePermissionResult>
{
    /// <summary>
    /// Handles the permission creation command.
    /// </summary>
    /// <param name="command">The command containing permission resource, action, and description.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="AdminCreatePermissionResult" /> containing the created permission.</returns>
    public async Task<AdminCreatePermissionResult> Handle(
        AdminCreatePermissionCommand command,
        CancellationToken cancellationToken
    )
    {
        bool permissionExists = await permissionRepository.ExistsByResourceAndActionAsync(
            action: command.Action,
            resource: command.Resource,
            cancellationToken: cancellationToken
        );

        if (permissionExists)
        {
            throw UserErrors.PermissionAlreadyExists(resource: command.Resource, action: command.Action);
        }

        var permission = PermissionEntity.Create(
            id: Guid.NewGuid(),
            action: command.Action,
            resource: command.Resource,
            description: command.Description
        );

        await permissionRepository.AddAsync(permission: permission, cancellationToken: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        var permissionDto = permission.ToPermissionDto(mapper);

        return new AdminCreatePermissionResult(Permission: permissionDto);
    }
}
