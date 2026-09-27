namespace _116.Content.Domain.Events;

/// <summary>
/// The paid-for effects of a single order item, computed when the order is
/// marked paid. The promotion window is resolved at raise time so a deferred
/// or redispatched consumer always applies the original paid-for window and
/// never recomputes it from a later "now".
/// </summary>
/// <param name="OrderItemId">The paid order item the effects apply to.</param>
/// <param name="PromotionLevelId">The purchased promotion level, or <c>null</c> when the item carries no promotion.</param>
/// <param name="PromotionUntil">When the purchased promotion expires, or <c>null</c> when the item carries no promotion.</param>
/// <param name="SocialBoost">Whether the item was bought with the social-boost add-on.</param>
public record PaidItemEffect(
    Guid OrderItemId,
    Guid? PromotionLevelId,
    DateTimeOffset? PromotionUntil,
    bool SocialBoost
);
