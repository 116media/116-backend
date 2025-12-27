using System.ComponentModel.DataAnnotations;

using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Shared.Errors;
using _116.Shared.Domain;

namespace _116.Identity.Domain.Entities;

/// <summary>
/// Represents a role that can be assigned to users and associated with permissions.
/// </summary>
public class RoleEntity : Aggregate<Guid>
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
    /// <exception cref="ArgumentException">Thrown when name or description are empty.</exception>
    public static RoleEntity Create(Guid id, string name, string description)
    {
        if (string.IsNullOrWhiteSpace(value: name))
        {
            throw UserErrors.RoleNameRequired();
        }

        if (string.IsNullOrWhiteSpace(value: description))
        {
            throw UserErrors.RoleDescriptionRequired();
        }

        return new RoleEntity { Id = id, Name = name, Description = description };
    }
}
