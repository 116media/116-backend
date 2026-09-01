using _116.Shared.Domain;

namespace _116.Content.Domain.Events;

/// <summary>
/// Raised whenever a pricing tier is created, renamed, activated or deactivated. Consumed by the lookup cache
/// invalidation; the consumer is idempotent, so repeated raises cost only a cache miss.
/// </summary>
/// <param name="PricingTierId">The pricing tier that changed.</param>
public record PricingTierChangedEvent(Guid PricingTierId) : DomainEvent;
