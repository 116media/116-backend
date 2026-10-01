using _116.Identity.Domain.Entities;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.AssignPermissionToRole.Contracts;

/// <summary>
/// The role carrying the fresh grant and the permission that was granted.
/// </summary>
/// <param name="Role">The role aggregate with the grant applied.</param>
/// <param name="Permission">The granted permission.</param>
public record PermissionGrantData(RoleEntity Role, PermissionEntity Permission);

/// <summary>
/// Resolves and applies a permission grant: loads the role and the permission, gates deleted and
/// inactive ones, and grants through the role aggregate.
/// </summary>
public interface IAdminAssignPermissionToRoleService
{
    /// <summary>
    /// Grants the permission to the role, throwing the localized error when either is deleted or
    /// inactive, or the permission is already assigned. The caller owns the commit.
    /// </summary>
    /// <param name="roleId">The role receiving the permission.</param>
    /// <param name="permissionId">The permission being granted.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The role with the grant applied and the granted permission.</returns>
    Task<PermissionGrantData> GrantAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken);
}
