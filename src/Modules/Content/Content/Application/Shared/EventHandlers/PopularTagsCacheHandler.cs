using _116.Content.Application.Shared.Cache;
using _116.Content.Domain.Events;
using _116.Shared.Application.Services;
using Microsoft.Extensions.Caching.Hybrid;

namespace _116.Content.Application.Shared.EventHandlers;

/// <summary>
/// Evicts the tags caches whenever the tag graph changes. The popular-tags
/// and all-tags projections share one tag, so a single eviction refreshes
/// both. Eviction is idempotent, so the multiple events raised by a bulk tag
/// replacement cost only a cache miss.
/// </summary>
/// <param name="cache">The hybrid cache holding the tag projections.</param>
public class PopularTagsCacheHandler(HybridCache cache) : IDomainEventHandler<TagGraphChangedEvent>
{
    /// <inheritdoc />
    public async Task Handle(TagGraphChangedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await cache.RemoveByTagAsync(ContentCacheTags.Tags, cancellationToken);
    }
}
