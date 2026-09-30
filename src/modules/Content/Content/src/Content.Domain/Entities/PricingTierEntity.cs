using System.ComponentModel.DataAnnotations;
using _116.Content.Domain.Constants;
using _116.Content.Domain.Events;
using _116.Content.Domain.Exceptions;
using _116.Content.Domain.StateMachines;
using _116.Shared.Domain;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Represents an add-on service fee tier used to price category content (e.g., "base_upload", "social_boost").
/// Pricing tiers are the building blocks of every category's price list.
/// </summary>
public partial class PricingTierEntity : Aggregate<Guid>
{
    /// <summary>
    /// Name of the pricing tier (e.g., "base_upload", "social_boost").
    /// </summary>
    [MaxLength(length: ContentConstants.MaxPricingTierNameLength)]
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Human-readable description explaining what this tier covers.
    /// </summary>
    [MaxLength(length: ContentConstants.MaxPricingTierDescriptionLength)]
    public string Description { get; private set; } = null!;

    /// <summary>
    /// Indicates whether this pricing tier is active and available for category pricing configuration.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Private parameterless constructor required by Entity Framework Core.
    /// </summary>
    private PricingTierEntity() { }

    /// <summary>
    /// Creates a new pricing tier entity.
    /// </summary>
    /// <param name="id">The unique identifier for the pricing tier.</param>
    /// <param name="name">The name of the pricing tier.</param>
    /// <param name="description">A description of what this tier covers.</param>
    /// <returns>A new <see cref="PricingTierEntity" /> instance.</returns>
    /// <exception cref="ContentRuleException">Thrown when name is empty or whitespace.</exception>
    public static PricingTierEntity Create(Guid id, string name, string description)
    {
        if (string.IsNullOrWhiteSpace(value: name))
        {
            throw new ContentRuleException(ContentRuleCodes.PricingTierNameRequired);
        }

        var pricingTier = new PricingTierEntity
        {
            Id = id,
            Name = name,
            Description = description,
        };
        pricingTier.AddDomainEvent(new PricingTierChangedEvent(PricingTierId: id));

        return pricingTier;
    }
}
