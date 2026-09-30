using _116.Content.Domain.Events;
using _116.Content.Domain.Exceptions;
using _116.Content.Domain.StateMachines;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Revision behaviour of <see cref="PromotionLevelEntity" />. Its state lives in <c>Entities/PromotionLevelEntity.cs</c>.
/// </summary>
public partial class PromotionLevelEntity
{
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
        AddDomainEvent(new PromotionLevelChangedEvent(PromotionLevelId: Id));
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
}
