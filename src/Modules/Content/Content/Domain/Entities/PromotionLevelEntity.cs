using System.ComponentModel.DataAnnotations;
using _116.Content.Application.Shared.Errors;
using _116.Content.Domain.Constants;
using _116.Shared.Domain;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Represents a homepage placement upgrade option (e.g., "Featured — 7 days", "À la Une — 14 days").
/// Promotion levels are upsell options available when a customer commissions content.
/// </summary>
public class PromotionLevelEntity : Aggregate<Guid>
{
    /// <summary>
    /// Display name of the promotion level (e.g., "Featured — 7 days").
    /// </summary>
    [MaxLength(length: ContentConstants.MaxPromotionLevelNameLength)]
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Duration of the homepage placement in days.
    /// </summary>
    public int DurationDays { get; private set; }

    /// <summary>
    /// Price of this promotion level in USD.
    /// </summary>
    public decimal PriceUsd { get; private set; }

    /// <summary>
    /// Indicates whether this promotion level is active and available for selection on new orders.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Private parameterless constructor required by Entity Framework Core.
    /// </summary>
    private PromotionLevelEntity() { }

    /// <summary>
    /// Creates a new promotion level entity.
    /// </summary>
    /// <param name="id">The unique identifier for the promotion level.</param>
    /// <param name="name">The display name of the promotion level.</param>
    /// <param name="durationDays">The placement duration in days (must be greater than zero).</param>
    /// <param name="priceUsd">The price in USD (must be zero or greater).</param>
    /// <returns>A new <see cref="PromotionLevelEntity" /> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when name is empty or constraints are violated.</exception>
    public static PromotionLevelEntity Create(Guid id, string name, int durationDays, decimal priceUsd)
    {
        if (string.IsNullOrWhiteSpace(value: name))
        {
            throw PromotionLevelErrors.NameRequired();
        }

        if (durationDays <= 0)
        {
            throw PromotionLevelErrors.DurationMustBePositive();
        }

        if (priceUsd < 0)
        {
            throw PromotionLevelErrors.PriceMustBeNonNegative();
        }

        return new PromotionLevelEntity
        {
            Id = id,
            Name = name,
            DurationDays = durationDays,
            PriceUsd = priceUsd,
        };
    }

    /// <summary>
    /// Updates the name, duration, and price of this promotion level.
    /// </summary>
    /// <param name="name">The new display name.</param>
    /// <param name="durationDays">The new placement duration in days.</param>
    /// <param name="priceUsd">The new price in USD.</param>
    public void Update(string name, int durationDays, decimal priceUsd)
    {
        if (string.IsNullOrWhiteSpace(value: name))
        {
            throw PromotionLevelErrors.NameRequired();
        }

        if (durationDays <= 0)
        {
            throw PromotionLevelErrors.DurationMustBePositive();
        }

        if (priceUsd < 0)
        {
            throw PromotionLevelErrors.PriceMustBeNonNegative();
        }

        Name = name;
        DurationDays = durationDays;
        PriceUsd = priceUsd;
    }

    /// <summary>
    /// Guards that this promotion level is active and available for selection on new orders.
    /// </summary>
    /// <exception cref="_116.Shared.Application.Exceptions.NotFoundException">
    /// Thrown when the promotion level is inactive, surfaced as a not-found error to avoid leaking state.
    /// </exception>
    public void EnsureActive()
    {
        if (!IsActive)
        {
            throw PromotionLevelErrors.NotFound(id: Id);
        }
    }

    /// <summary>
    /// Activates the promotion level, making it available for selection on new orders.
    /// </summary>
    /// <returns>True if the promotion level was activated, false if already active.</returns>
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
    /// Deactivates the promotion level, hiding it from the order form for new orders.
    /// </summary>
    /// <returns>True if the promotion level was deactivated, false if already inactive.</returns>
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
