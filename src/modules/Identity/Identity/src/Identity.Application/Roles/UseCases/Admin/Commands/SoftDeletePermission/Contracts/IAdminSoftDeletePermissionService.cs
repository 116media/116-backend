using _116.Identity.Domain.Entities;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.SoftDeletePermission.Contracts;

/// <summary>
/// Loads a permission and applies the clocked soft deletion.
/// </summary>
public interface IAdminSoftDeletePermissionService
{
    /// <summary>
    /// Soft-deletes the permission, throwing the localized error when it is already deleted.
    /// The caller owns the commit.
    /// </summary>
    /// <param name="permissionId">The permission being deleted.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The permission with the deletion applied.</returns>
    Task<PermissionEntity> SoftDeleteAsync(Guid permissionId, CancellationToken cancellationToken);
}
