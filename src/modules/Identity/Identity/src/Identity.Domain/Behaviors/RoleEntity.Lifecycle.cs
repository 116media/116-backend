using _116.Identity.Domain.Events;
using _116.Identity.Domain.Exceptions;
using _116.Identity.Domain.StateMachines;

namespace _116.Identity.Domain.Entities;

/// <summary>
/// Lifecycle transitions of <see cref="RoleEntity" />. Its state lives in <c>Entities/RoleEntity.cs</c>.
/// </summary>
public partial class RoleEntity
{
    /// <summary>
    /// Updates the role's name and description, raising <see cref="RoleChangedEvent" /> only
    /// when a value actually changed. Idempotent: identical values report <c>false</c>.
    /// </summary>
    /// <param name="name">The new name for the role.</param>
    /// <param name="description">The new description for the role.</param>
    /// <returns><c>true</c> if a value changed; <c>false</c> otherwise.</returns>
    public bool Update(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(value: name))
        {
            throw new IdentityRuleException(IdentityRuleCodes.RoleNameRequired);
        }

        if (string.IsNullOrWhiteSpace(value: description))
        {
            throw new IdentityRuleException(IdentityRuleCodes.RoleDescriptionRequired);
        }

        if (Name == name && Description == description)
        {
            return false;
        }

        Name = name;
        Description = description;
        AddDomainEvent(new RoleChangedEvent(RoleId: Id));
        return true;
    }

    /// <summary>
    /// Activates the role, allowing it to be assigned to users.
    /// </summary>
    /// <returns>True if the role was activated, false if already active.</returns>
    public bool Activate()
    {
        if (IsActive)
        {
            return false;
        }

        IsActive = true;
        AddDomainEvent(new RoleChangedEvent(RoleId: Id));

        return true;
    }

    /// <summary>
    /// Deactivates the role, preventing it from being assigned to users.
    /// </summary>
    /// <returns>True if the role was deactivated, false if already inactive.</returns>
    public bool Deactivate()
    {
        if (!IsActive)
        {
            return false;
        }

        IsActive = false;
        AddDomainEvent(new RoleChangedEvent(RoleId: Id));

        return true;
    }

    /// <summary>
    /// Marks the role as deleted (soft delete).
    /// </summary>
    /// <param name="now">The current UTC instant, stamped as the deletion time.</param>
    /// <returns>True if the role was soft-deleted, false if already deleted.</returns>
    public bool SoftDelete(DateTime now)
    {
        if (IsDeleted)
        {
            return false;
        }

        IsDeleted = true;
        IsActive = false;
        DeletedAt = now;
        AddDomainEvent(new RoleChangedEvent(RoleId: Id));

        return true;
    }

    /// <summary>
    /// Restores a soft-deleted role.
    /// </summary>
    /// <returns>True if the role was restored, false if not deleted.</returns>
    public bool Restore()
    {
        if (!IsDeleted)
        {
            return false;
        }

        IsDeleted = false;
        DeletedAt = null;
        AddDomainEvent(new RoleChangedEvent(RoleId: Id));

        return true;
    }

    /// <summary>
    /// Raises the role-changed fact for this role's permanent removal, so cached lookup
    /// projections refresh once the deletion commits. The fact is declared here because the
    /// removal destroys the aggregate itself, leaving no state to transition (D14).
    /// </summary>
    public void MarkHardDeleted()
    {
        AddDomainEvent(new RoleChangedEvent(RoleId: Id));
    }
}
