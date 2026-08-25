using _116.Content.Application.Shared.Cache;
using _116.Content.Domain.Events;
using _116.Shared.Application.Services;
using Microsoft.Extensions.Caching.Hybrid;

namespace _116.Content.Application.Shared.EventHandlers;

/// <summary>
/// Evicts the cached lookup tables — content types, pricing tiers, promotion levels and
/// categories — whenever any lookup aggregate changes. Eviction is idempotent, so handling
/// the same fact more than once costs only a cache miss.
/// </summary>
/// <param name="cache">The hybrid cache holding the lookup projections.</param>
public class LookupCacheHandler(HybridCache cache)
    : IDomainEventHandler<ContentTypeChangedEvent>,
        IDomainEventHandler<PricingTierChangedEvent>,
        IDomainEventHandler<PromotionLevelChangedEvent>,
        IDomainEventHandler<CategoryChangedEvent>
{
    /// <inheritdoc />
    public async Task Handle(ContentTypeChangedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await cache.RemoveByTagAsync(ContentCacheTags.Lookups, cancellationToken);
    }

    /// <inheritdoc />
    public async Task Handle(PricingTierChangedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await cache.RemoveByTagAsync(ContentCacheTags.Lookups, cancellationToken);
    }

    /// <inheritdoc />
    public async Task Handle(PromotionLevelChangedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await cache.RemoveByTagAsync(ContentCacheTags.Lookups, cancellationToken);
    }

    /// <inheritdoc />
    public async Task Handle(CategoryChangedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await cache.RemoveByTagAsync(ContentCacheTags.Lookups, cancellationToken);
    }
}
