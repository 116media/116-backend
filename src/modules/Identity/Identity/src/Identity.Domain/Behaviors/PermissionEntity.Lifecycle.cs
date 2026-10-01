using _116.Identity.Domain.Events;
using _116.Identity.Domain.Exceptions;
using _116.Identity.Domain.StateMachines;

namespace _116.Identity.Domain.Entities;

/// <summary>
/// Lifecycle transitions of <see cref="PermissionEntity" />. Its state lives in <c>Entities/PermissionEntity.cs</c>.
/// </summary>
public partial class PermissionEntity
{
    /// <summary>
    /// Updates the permission's resource, action, and description, raising
    /// <see cref="PermissionChangedEvent" /> only when a value actually changed.
    /// Idempotent: identical values report <c>false</c>.
    /// </summary>
    /// <param name="resource">The new resource name.</param>
    /// <param name="action">The new action name.</param>
    /// <param name="description">The new description.</param>
    /// <returns><c>true</c> if a value changed; <c>false</c> otherwise.</returns>
    public bool Update(string resource, string action, string description)
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

        if (Resource == resource && Action == action && Description == description)
        {
            return false;
        }

        Resource = resource;
        Action = action;
        Description = description;
        AddDomainEvent(new PermissionChangedEvent(PermissionId: Id));
        return true;
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
        AddDomainEvent(new PermissionChangedEvent(PermissionId: Id));

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
        AddDomainEvent(new PermissionChangedEvent(PermissionId: Id));

        return true;
    }

    /// <summary>
    /// Marks the permission as deleted (soft delete).
    /// </summary>
    /// <param name="now">The current UTC instant, stamped as the deletion time.</param>
    /// <returns>True if the permission was soft-deleted, false if already deleted.</returns>
    public bool SoftDelete(DateTime now)
    {
        if (IsDeleted)
        {
            return false;
        }

        IsDeleted = true;
        IsActive = false;
        DeletedAt = now;
        AddDomainEvent(new PermissionChangedEvent(PermissionId: Id));

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
        AddDomainEvent(new PermissionChangedEvent(PermissionId: Id));

        return true;
    }

    /// <summary>
    /// Raises the permission-changed fact for this permission's permanent removal, so cached
    /// lookup projections refresh once the deletion commits. The fact is declared here because
    /// the removal destroys the aggregate itself, leaving no state to transition (D14).
    /// </summary>
    public void MarkHardDeleted()
    {
        AddDomainEvent(new PermissionChangedEvent(PermissionId: Id));
    }
}
