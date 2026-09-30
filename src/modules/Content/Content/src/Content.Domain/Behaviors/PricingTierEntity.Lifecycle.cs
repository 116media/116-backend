using _116.Content.Domain.Events;
using _116.Content.Domain.Exceptions;
using _116.Content.Domain.StateMachines;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Lifecycle transitions of <see cref="PricingTierEntity" />. Its state lives in <c>Entities/PricingTierEntity.cs</c>.
/// </summary>
public partial class PricingTierEntity
{
    /// <summary>
    /// Updates the name and description of this pricing tier.
    /// </summary>
    /// <param name="name">The new name for the pricing tier.</param>
    /// <param name="description">The new description for the pricing tier.</param>
    public void Update(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(value: name))
        {
            throw new ContentRuleException(ContentRuleCodes.PricingTierNameRequired);
        }

        Name = name;
        Description = description;
        AddDomainEvent(new PricingTierChangedEvent(PricingTierId: Id));
    }

    /// <summary>
    /// Activates the pricing tier, making it available for category pricing configuration.
    /// </summary>
    /// <returns>True if the pricing tier was activated, false if already active.</returns>
    public bool Activate()
    {
        if (IsActive)
        {
            return false;
        }

        IsActive = true;
        AddDomainEvent(new PricingTierChangedEvent(PricingTierId: Id));

        return true;
    }

    /// <summary>
    /// Deactivates the pricing tier, removing it from the category pricing configuration form.
    /// </summary>
    /// <returns>True if the pricing tier was deactivated, false if already inactive.</returns>
    public bool Deactivate()
    {
        if (!IsActive)
        {
            return false;
        }

        IsActive = false;
        AddDomainEvent(new PricingTierChangedEvent(PricingTierId: Id));

        return true;
    }
}
