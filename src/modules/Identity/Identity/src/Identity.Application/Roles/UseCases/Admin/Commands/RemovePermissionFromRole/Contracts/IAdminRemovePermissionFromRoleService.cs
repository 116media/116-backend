using _116.Identity.Domain.Entities;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.RemovePermissionFromRole.Contracts;

/// <summary>
/// Resolves and applies a permission revocation through the role aggregate.
/// </summary>
public interface IAdminRemovePermissionFromRoleService
{
    /// <summary>
    /// Revokes the permission from the role, throwing the localized error when it is not
    /// assigned. The caller owns the commit.
    /// </summary>
    /// <param name="roleId">The role losing the permission.</param>
    /// <param name="permissionId">The permission being revoked.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The role with the revocation applied.</returns>
    Task<RoleEntity> RevokeAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken);
}
