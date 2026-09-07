using _116.Shared.Domain;

namespace _116.Content.Domain.Events;

/// <summary>
/// Raised whenever a promotion level is created, updated, activated or deactivated. Consumed by the lookup cache
/// invalidation; the consumer is idempotent, so repeated raises cost only a cache miss.
/// </summary>
/// <param name="PromotionLevelId">The promotion level that changed.</param>
public record PromotionLevelChangedEvent(Guid PromotionLevelId) : IDomainEvent;
