using System.ComponentModel.DataAnnotations;
using _116.Content.Domain.Constants;
using _116.Content.Domain.Exceptions;
using _116.Content.Domain.StateMachines;
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
    /// Homepage grid spot this promotion level targets (1 = hero top-left, 2 = tall side
    /// top-right, 3 = small pair bottom-left). Controls which carousel the promoted article
    /// or video appears in on the homepage feed. Null means no specific spot is targeted.
    /// </summary>
    public int? SpotPriority { get; private set; }

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
    /// <exception cref="ContentRuleException">Thrown when name is empty or constraints are violated.</exception>
    public static PromotionLevelEntity Create(
        Guid id,
        string name,
        int durationDays,
        decimal priceUsd,
        int? spotPriority
    )
    {
        if (string.IsNullOrWhiteSpace(value: name))
        {
            throw new ContentRuleException(ContentRuleCodes.PromotionLevelNameRequired);
        }

        if (durationDays <= 0)
        {
            throw new ContentRuleException(ContentRuleCodes.PromotionLevelDurationMustBePositive);
        }

        if (priceUsd < 0)
        {
            throw new ContentRuleException(ContentRuleCodes.PromotionLevelPriceMustBeNonNegative);
        }

        if (spotPriority is < 1 or > 3)
        {
            throw new ContentRuleException(ContentRuleCodes.PromotionLevelInvalidSpotPriority);
        }

        return new PromotionLevelEntity
        {
            Id = id,
            Name = name,
            DurationDays = durationDays,
            PriceUsd = priceUsd,
            SpotPriority = spotPriority,
        };
    }

    /// <summary>
    /// Updates the name, duration, and price of this promotion level.
    /// </summary>
    /// <param name="name">The new display name.</param>
    /// <param name="durationDays">The new placement duration in days.</param>
    /// <param name="priceUsd">The new price in USD.</param>
    public void Update(string name, int durationDays, decimal priceUsd, int? spotPriority)
    {
        if (string.IsNullOrWhiteSpace(value: name))
        {
            throw new ContentRuleException(ContentRuleCodes.PromotionLevelNameRequired);
        }

        if (durationDays <= 0)
        {
            throw new ContentRuleException(ContentRuleCodes.PromotionLevelDurationMustBePositive);
        }

        if (priceUsd < 0)
        {
            throw new ContentRuleException(ContentRuleCodes.PromotionLevelPriceMustBeNonNegative);
        }

        if (spotPriority is < 1 or > 3)
        {
            throw new ContentRuleException(ContentRuleCodes.PromotionLevelInvalidSpotPriority);
        }

        Name = name;
        DurationDays = durationDays;
        PriceUsd = priceUsd;
        SpotPriority = spotPriority;
    }

    /// <summary>
    /// Guards that this promotion level is active and available for selection on new orders.
    /// </summary>
    /// <exception cref="ContentRuleException">
    /// Thrown when the promotion level is inactive, surfaced as a not-found error to avoid leaking state.
    /// </exception>
    public void EnsureActive()
    {
        if (!IsActive)
        {
            throw new ContentRuleException(ContentRuleCodes.PromotionLevelNotFound, Id.ToString());
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
