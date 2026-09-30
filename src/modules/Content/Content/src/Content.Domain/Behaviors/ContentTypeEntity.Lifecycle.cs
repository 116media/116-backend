using _116.Content.Domain.Events;
using _116.Content.Domain.Exceptions;
using _116.Content.Domain.StateMachines;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Lifecycle transitions of <see cref="ContentTypeEntity" />. Its state lives in <c>Entities/ContentTypeEntity.cs</c>.
/// </summary>
public partial class ContentTypeEntity
{
    /// <summary>
    /// Updates the content type name.
    /// </summary>
    /// <param name="name">The new display name of the content type.</param>
    /// <exception cref="ContentRuleException">Thrown when name is empty or whitespace.</exception>
    public void Update(string name)
    {
        if (string.IsNullOrWhiteSpace(value: name))
        {
            throw new ContentRuleException(ContentRuleCodes.ContentTypeNameRequired);
        }

        Name = name;
        AddDomainEvent(new ContentTypeChangedEvent(ContentTypeId: Id));
    }

    /// <summary>
    /// Activates the content type, making it selectable when creating new categories.
    /// </summary>
    /// <returns>True if the content type was activated, false if already active.</returns>
    public bool Activate()
    {
        if (IsActive)
        {
            return false;
        }

        IsActive = true;
        AddDomainEvent(new ContentTypeChangedEvent(ContentTypeId: Id));

        return true;
    }

    /// <summary>
    /// Deactivates the content type, preventing it from being assigned to new categories.
    /// </summary>
    /// <returns>True if the content type was deactivated, false if already inactive.</returns>
    public bool Deactivate()
    {
        if (!IsActive)
        {
            return false;
        }

        IsActive = false;
        AddDomainEvent(new ContentTypeChangedEvent(ContentTypeId: Id));

        return true;
    }
}
