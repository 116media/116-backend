using System.ComponentModel.DataAnnotations;
using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Shared.Errors;
using _116.Shared.Application.Exceptions;
using _116.Shared.Domain;

namespace _116.Identity.Domain.Entities;

/// <summary>
/// Represents a permission that defines an action allowed on a specific resource.
/// </summary>
/// <remarks>
/// Permissions are typically associated with roles through <see cref="RolePermissionEntity" />.
/// </remarks>
public class PermissionEntity : Aggregate<Guid>
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
    /// <exception cref="ArgumentException">Thrown when parameters are null, empty, or exceed maximum length.</exception>
    public static PermissionEntity Create(Guid id, string resource, string action, string description)
    {
        BadRequestException? error = (resource, action, description) switch
        {
            var (r, _, _) when string.IsNullOrWhiteSpace(value: r) => UserErrors.PermissionResourceRequired(),
            var (_, a, _) when string.IsNullOrWhiteSpace(value: a) => UserErrors.PermissionActionRequired(),
            var (_, _, d) when string.IsNullOrWhiteSpace(value: d) => UserErrors.PermissionDescriptionRequired(),
            _ => null,
        };

        if (error is not null)
        {
            throw error;
        }

        return new PermissionEntity
        {
            Id = id,
            Resource = resource,
            Action = action,
            Description = description,
        };
    }

    /// <summary>
    /// Updates the permission's resource, action, and description.
    /// </summary>
    /// <param name="resource">The new resource name.</param>
    /// <param name="action">The new action name.</param>
    /// <param name="description">The new description.</param>
    public void Update(string resource, string action, string description)
    {
        BadRequestException? error = (resource, action, description) switch
        {
            var (r, _, _) when string.IsNullOrWhiteSpace(value: r) => UserErrors.PermissionResourceRequired(),
            var (_, a, _) when string.IsNullOrWhiteSpace(value: a) => UserErrors.PermissionActionRequired(),
            var (_, _, d) when string.IsNullOrWhiteSpace(value: d) => UserErrors.PermissionDescriptionRequired(),
            _ => null,
        };

        if (error is not null)
        {
            throw error;
        }

        Resource = resource;
        Action = action;
        Description = description;
    }

    /// <summary>
    /// Activates the permission, allowing it to be used.
    /// </summary>
    /// <returns>True if the permission was activated, false if already active.</returns>
    public bool Activate()
    {
        if (IsActive)
        {
            return false;
        }

        IsActive = true;
        return true;
    }

    /// <summary>
    /// Deactivates the permission, preventing it from being used.
    /// </summary>
    /// <returns>True if the permission was deactivated, false if already inactive.</returns>
    public bool Deactivate()
    {
        if (!IsActive)
        {
            return false;
        }

        IsActive = false;
        return true;
    }

    /// <summary>
    /// Marks the permission as deleted (soft delete).
    /// </summary>
    /// <returns>True if the permission was soft-deleted, false if already deleted.</returns>
    public bool SoftDelete()
    {
        if (IsDeleted)
        {
            return false;
        }

        IsDeleted = true;
        IsActive = false;
        DeletedAt = DateTime.UtcNow;
        return true;
    }

    /// <summary>
    /// Restores a soft-deleted permission.
    /// </summary>
    /// <returns>True if the permission was restored, false if not deleted.</returns>
    public bool Restore()
    {
        if (!IsDeleted)
        {
            return false;
        }

        IsDeleted = false;
        DeletedAt = null;
        return true;
    }
}
