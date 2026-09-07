using _116.Identity.Domain.Events;
using _116.Shared.Domain;

namespace _116.Identity.Domain.Entities;

/// <summary>
/// Represents the many-to-many association between roles and permissions.
/// </summary>
public class RolePermissionEntity : Aggregate<Guid>
{
    /// <summary>
    /// Foreign key referencing the associated role.
    /// </summary>
    public Guid RoleId { get; private set; }

    /// <summary>
    /// Foreign key referencing the associated permission.
    /// </summary>
    public Guid PermissionId { get; private set; }

    /// <summary>
    /// Navigation property for the associated role.
    /// </summary>
    public RoleEntity Role { get; private set; } = null!;

    /// <summary>
    /// Navigation property for the associated permission.
    /// </summary>
    public PermissionEntity Permission { get; private set; } = null!;

    /// <summary>
    /// Creates a new role-permission association.
    /// </summary>
    /// <param name="id">The unique identifier of the association.</param>
    /// <param name="roleId">The ID of the role.</param>
    /// <param name="permissionId">The ID of the permission.</param>
    /// <returns>A new <see cref="RolePermissionEntity" /> instance.</returns>
    public static RolePermissionEntity Create(Guid id, Guid roleId, Guid permissionId)
    {
        var rolePermission = new RolePermissionEntity
        {
            Id = id,
            RoleId = roleId,
            PermissionId = permissionId,
        };
        rolePermission.AddDomainEvent(new RoleChangedEvent(RoleId: roleId));

        return rolePermission;
    }

    /// <summary>
    /// Raises the role-changed fact for this association's removal, so cached role
    /// projections refresh once the deletion commits.
    /// </summary>
    public void MarkRemoved()
    {
        AddDomainEvent(new RoleChangedEvent(RoleId: RoleId));
    }
}
