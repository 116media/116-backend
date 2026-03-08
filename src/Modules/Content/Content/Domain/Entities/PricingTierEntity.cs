using System.ComponentModel.DataAnnotations;
using _116.Content.Application.Shared.Errors;
using _116.Content.Domain.Constants;
using _116.Shared.Domain;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Represents an add-on service fee tier used to price category content (e.g., "base_upload", "social_boost").
/// Pricing tiers are the building blocks of every category's price list.
/// </summary>
public class PricingTierEntity : Aggregate<Guid>
{
    /// <summary>
    /// Name of the pricing tier (e.g., "base_upload", "social_boost").
    /// </summary>
    [MaxLength(length: ContentConstants.MaxPricingTierNameLength)]
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Optional human-readable description explaining what this tier covers.
    /// </summary>
    [MaxLength(length: ContentConstants.MaxPricingTierDescriptionLength)]
    public string? Description { get; private set; }

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
    /// <param name="description">An optional description of what this tier covers.</param>
    /// <returns>A new <see cref="PricingTierEntity" /> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when name is empty or whitespace.</exception>
    public static PricingTierEntity Create(Guid id, string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(value: name))
        {
            throw PricingTierErrors.NameRequired();
        }

        return new PricingTierEntity
        {
            Id = id,
            Name = name,
            Description = description,
        };
    }

    /// <summary>
    /// Updates the name and description of this pricing tier.
    /// </summary>
    /// <param name="name">The new name for the pricing tier.</param>
    /// <param name="description">The new description (may be null).</param>
    public void Update(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(value: name))
        {
            throw PricingTierErrors.NameRequired();
        }

        Name = name;
        Description = description;
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
        return true;
    }
}
