using System.ComponentModel.DataAnnotations;
using _116.Identity.Domain.Constants;
using _116.Identity.Domain.Events;
using _116.Identity.Domain.Exceptions;
using _116.Identity.Domain.StateMachines;
using _116.Shared.Domain;

namespace _116.Identity.Domain.Entities;

/// <summary>
/// Represents a role that can be assigned to users and associated with permissions.
/// </summary>
public partial class RoleEntity : Aggregate<Guid>
{
    /// <summary>
    /// Name of the role (e.g., "Admin", "Editor").
    /// </summary>
    [MaxLength(length: RoleConstants.MaxRoleNameLength)]
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Human-readable description of the role's purpose or scope.
    /// </summary>
    [MaxLength(length: RoleConstants.MaxRoleDescriptionLength)]
    public string Description { get; private set; } = null!;

    /// <summary>
    /// Indicates whether the role is active and can be assigned to users.
    /// </summary>
    public bool IsActive { get; private set; } = RoleConstants.DefaultIsActive;

    /// <summary>
    /// Indicates whether the role has been soft-deleted.
    /// </summary>
    public bool IsDeleted { get; private set; } = RoleConstants.DefaultIsDeleted;

    /// <summary>
    /// Date and time when the role was soft-deleted, in UTC.
    /// </summary>
    public DateTime? DeletedAt { get; private set; }

    /// <summary>
    /// Navigation property:
    /// Collection of user-role associations linking users to this role.
    /// </summary>
    public ICollection<UserRoleEntity> UserRoles { get; private set; } = new List<UserRoleEntity>();

    /// <summary>
    /// Navigation property:
    /// Collection of role-permission associations linking this role to its permissions.
    /// </summary>
    public ICollection<RolePermissionEntity> RolePermissions { get; private set; } = new List<RolePermissionEntity>();

    /// <summary>
    /// Creates a new role entity.
    /// </summary>
    /// <param name="id">The unique identifier of the role.</param>
    /// <param name="name">The name of the role.</param>
    /// <param name="description">The description of the role's purpose.</param>
    /// <returns>A new <see cref="RoleEntity" /> instance.</returns>
    /// <exception cref="IdentityRuleException">Thrown when name or description are empty.</exception>
    public static RoleEntity Create(Guid id, string name, string description)
    {
        if (string.IsNullOrWhiteSpace(value: name))
        {
            throw new IdentityRuleException(IdentityRuleCodes.RoleNameRequired);
        }

        if (string.IsNullOrWhiteSpace(value: description))
        {
            throw new IdentityRuleException(IdentityRuleCodes.RoleDescriptionRequired);
        }

        var role = new RoleEntity
        {
            Id = id,
            Name = name,
            Description = description,
        };
        role.AddDomainEvent(new RoleChangedEvent(RoleId: id));

        return role;
    }
}
