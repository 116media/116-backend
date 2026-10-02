using System.ComponentModel.DataAnnotations;
using _116.Identity.Domain.Constants;
using _116.Identity.Domain.Events;
using _116.Identity.Domain.Exceptions;
using _116.Identity.Domain.StateMachines;
using _116.Shared.Domain;

namespace _116.Identity.Domain.Entities;

/// <summary>
/// Represents a permission that defines an action allowed on a specific resource.
/// </summary>
/// <remarks>
/// Permissions are typically associated with roles through <see cref="RolePermissionEntity" />.
/// </remarks>
public partial class PermissionEntity : Aggregate<Guid>
{
    /// <summary>
    /// The name or key of the resource (e.g., "user", "receipt", "article").
    /// </summary>
    [MaxLength(length: PermissionConstants.MaxPermissionResourceLength)]
    public string Resource { get; private set; } = null!;

    /// <summary>
    /// The type of action allowed on the resource (e.g., "read", "create", "approve").
    /// </summary>
    [MaxLength(length: PermissionConstants.MaxPermissionActionLength)]
    public string Action { get; private set; } = null!;

    /// <summary>
    /// Human-readable description of the permission's purpose or scope.
    /// </summary>
    [MaxLength(length: PermissionConstants.MaxPermissionDescriptionLength)]
    public string Description { get; private set; } = null!;

    /// <summary>
    /// Indicates whether the permission is active and can be used.
    /// </summary>
    public bool IsActive { get; private set; } = PermissionConstants.DefaultIsActive;

    /// <summary>
    /// Indicates whether the permission has been soft-deleted.
    /// </summary>
    public bool IsDeleted { get; private set; } = PermissionConstants.DefaultIsDeleted;

    /// <summary>
    /// Date and time when the permission was soft-deleted, in UTC.
    /// </summary>
    public DateTime? DeletedAt { get; private set; }

    /// <summary>
    /// Navigation property:
    /// Collection of role-permission associations linked to this permission.
    /// </summary>
    public ICollection<RolePermissionEntity> RolePermissions { get; private set; } = new List<RolePermissionEntity>();

    /// <summary>
    /// Creates a new permission entity.
    /// </summary>
    /// <param name="id">The unique identifier of the permission.</param>
    /// <param name="resource">The resource name (e.g., "user", "file", "receipt").</param>
    /// <param name="action">The action name (e.g., "read", "create", "delete").</param>
    /// <param name="description">The description of the permission's purpose.</param>
    /// <returns>A new <see cref="PermissionEntity" /> instance.</returns>
    /// <exception cref="IdentityRuleException">Thrown when parameters are null, empty, or exceed maximum length.</exception>
    public static PermissionEntity Create(Guid id, string resource, string action, string description)
    {
        Exception? error = (resource, action, description) switch
        {
            var (r, _, _) when string.IsNullOrWhiteSpace(value: r) => new IdentityRuleException(
                IdentityRuleCodes.PermissionResourceRequired
            ),
            var (_, a, _) when string.IsNullOrWhiteSpace(value: a) => new IdentityRuleException(
                IdentityRuleCodes.PermissionActionRequired
            ),
            var (_, _, d) when string.IsNullOrWhiteSpace(value: d) => new IdentityRuleException(
                IdentityRuleCodes.PermissionDescriptionRequired
            ),
            _ => null,
        };

        if (error is not null)
        {
            throw error;
        }

        var permission = new PermissionEntity
        {
            Id = id,
            Resource = resource,
            Action = action,
            Description = description,
        };
        permission.AddDomainEvent(new PermissionChangedEvent(PermissionId: id));

        return permission;
    }
}
