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
        return new RolePermissionEntity { Id = id, RoleId = roleId, PermissionId = permissionId };
    }
}
