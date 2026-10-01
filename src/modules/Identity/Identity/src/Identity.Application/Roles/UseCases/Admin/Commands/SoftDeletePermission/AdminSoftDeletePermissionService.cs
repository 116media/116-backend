using _116.Identity.Application.Roles.UseCases.Admin.Commands.SoftDeletePermission.Contracts;
using _116.Identity.Application.Shared.Errors.Facade;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.SoftDeletePermission;

/// <summary>
/// Loads the permission and applies the clocked soft deletion for the admin soft-delete use case.
/// </summary>
/// <param name="permissionRepository">Repository for permission data access operations.</param>
/// <param name="i18n">Single i18n entry point for the Identity module.</param>
/// <param name="timeProvider">Clock supplying the deletion instant.</param>
public class AdminSoftDeletePermissionService(
    IPermissionRepository permissionRepository,
    IdentityI18n i18n,
    TimeProvider timeProvider
) : IAdminSoftDeletePermissionService
{
    /// <inheritdoc />
    public async Task<PermissionEntity> SoftDeleteAsync(Guid permissionId, CancellationToken cancellationToken)
    {
        PermissionEntity? permission = await permissionRepository.GetPermissionByIdOrThrowAsync(
            permissionId: permissionId,
            cancellationToken: cancellationToken
        );

        if (!permission!.SoftDelete(now: timeProvider.GetUtcNow().UtcDateTime))
        {
            throw i18n.User.PermissionAlreadyDeleted();
        }

        return permission;
    }
}
