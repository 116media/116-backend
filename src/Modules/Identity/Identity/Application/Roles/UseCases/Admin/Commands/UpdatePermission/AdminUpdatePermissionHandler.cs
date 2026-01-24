using _116.Identity.Application.Shared.Errors;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.UpdatePermission;

/// <summary>
/// Handles the <see cref="AdminUpdatePermissionCommand" /> to update an existing permission.
/// </summary>
/// <param name="permissionRepository">Repository for permission data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminUpdatePermissionHandler(IPermissionRepository permissionRepository, IIdentityUnitOfWork unitOfWork)
    : ICommandHandler<AdminUpdatePermissionCommand, AdminUpdatePermissionResult>
{
    /// <summary>
    /// Handles the permission update command.
    /// </summary>
    /// <param name="command">The command containing permission ID and update data.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="AdminUpdatePermissionResult" /> containing the updated permission.</returns>
    public async Task<AdminUpdatePermissionResult> Handle(
        AdminUpdatePermissionCommand command,
        CancellationToken cancellationToken
    )
    {
        PermissionEntity? permission = await permissionRepository.GetPermissionByIdOrThrowAsync(
            permissionId: command.PermissionId,
            cancellationToken: cancellationToken
        );

        string? action = command.Action;
        string? resource = command.Resource;
        string? description = command.Description;

        bool isActionUpdated = !string.IsNullOrEmpty(value: action) && permission!.Action != action;
        bool isResourceUpdated = !string.IsNullOrEmpty(value: resource) && permission!.Resource != resource;
        bool isDescriptionUpdated = !string.IsNullOrEmpty(value: description) && permission!.Description != description;

        // Check uniqueness if resource or action is being changed
        if (isResourceUpdated || isActionUpdated)
        {
            string newAction = isActionUpdated ? action! : permission!.Action;
            string newResource = isResourceUpdated ? resource! : permission!.Resource;

            await EnsurePermissionUnique(
                action: newAction,
                resource: newResource,
                cancellationToken: cancellationToken
            );
        }

        if (isResourceUpdated || isActionUpdated || isDescriptionUpdated)
        {
            string newAction = isActionUpdated ? action! : permission!.Action;
            string newResource = isResourceUpdated ? resource! : permission!.Resource;
            string newDescription = isDescriptionUpdated ? description! : permission!.Description;

            permission!.Update(resource: newResource, action: newAction, description: newDescription);
            await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
        }

        var permissionDto = permission!.ToPermissionDto();
        return new AdminUpdatePermissionResult(Permission: permissionDto);
    }

    private async Task EnsurePermissionUnique(string resource, string action, CancellationToken cancellationToken)
    {
        if (
            await permissionRepository.ExistsByResourceAndActionAsync(
                resource: resource,
                action: action,
                cancellationToken: cancellationToken
            )
        )
        {
            throw UserErrors.PermissionAlreadyExists(resource: resource, action: action);
        }
    }
}
