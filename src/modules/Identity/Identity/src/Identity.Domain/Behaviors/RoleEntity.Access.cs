using _116.Identity.Domain.Events;

namespace _116.Identity.Domain.Entities;

/// <summary>
/// Permission grants of <see cref="RoleEntity" />. Its state lives in <c>Entities/RoleEntity.cs</c>.
/// </summary>
public partial class RoleEntity
{
    // Permission Methods
    /// <summary>
    /// Grants a permission to this role and raises <see cref="RoleChangedEvent" />.
    /// Idempotent: a permission already granted reports <c>false</c> and raises nothing.
    /// </summary>
    /// <param name="permissionId">The ID of the permission to grant.</param>
    /// <returns><c>true</c> if the permission was granted; <c>false</c> if already granted.</returns>
    public bool GrantPermission(Guid permissionId)
    {
        if (HasPermission(permissionId: permissionId))
        {
            return false;
        }

        RolePermissions.Add(item: RolePermissionEntity.Create(roleId: Id, permissionId: permissionId));

        AddDomainEvent(new RoleChangedEvent(RoleId: Id));
        return true;
    }

    /// <summary>
    /// Revokes a permission from this role and raises <see cref="RoleChangedEvent" />.
    /// Idempotent: a permission not granted reports <c>false</c> and raises nothing.
    /// </summary>
    /// <param name="permissionId">The ID of the permission to revoke.</param>
    /// <returns><c>true</c> if the permission was revoked; <c>false</c> if it was not granted.</returns>
    public bool RevokePermission(Guid permissionId)
    {
        RolePermissionEntity? rolePermission = RolePermissions.FirstOrDefault(rp => rp.PermissionId == permissionId);
        if (rolePermission is null)
        {
            return false;
        }

        RolePermissions.Remove(item: rolePermission);

        AddDomainEvent(new RoleChangedEvent(RoleId: Id));
        return true;
    }

    /// <summary>
    /// Checks if this role has a specific permission.
    /// </summary>
    public bool HasPermission(Guid permissionId)
    {
        return RolePermissions.Any(rp => rp.PermissionId == permissionId);
    }
}
