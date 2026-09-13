using _116.Shared.Domain;

namespace _116.Identity.Domain.Entities;

/// <summary>
/// Member entity of the Role aggregate recording a single permission grant.
/// Created and removed only through <see cref="RoleEntity" />'s grant/revoke methods.
/// </summary>
public class RolePermissionEntity : Entity<Guid>
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
    /// Creates a role-permission association. The role-changed fact is raised by the role
    /// aggregate, not here — a member entity carries no events. The key stays unset so change
    /// detection tracks a row reached through the role's collection as an insert.
    /// </summary>
    /// <param name="roleId">The ID of the role.</param>
    /// <param name="permissionId">The ID of the permission.</param>
    /// <returns>A new <see cref="RolePermissionEntity" /> instance.</returns>
    internal static RolePermissionEntity Create(Guid roleId, Guid permissionId)
    {
        return new RolePermissionEntity { RoleId = roleId, PermissionId = permissionId };
    }
}
