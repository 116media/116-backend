using _116.Content.Application.Shared.Cache;
using _116.Content.Domain.Events;
using _116.Shared.Application.Services;

namespace _116.Content.Application.Shared.EventHandlers;

/// <summary>
/// Evicts the tags caches whenever the tag graph changes. The popular-tags
/// and all-tags projections share one eviction token, so a single
/// invalidation refreshes both. Eviction is idempotent, so the multiple
/// events raised by a bulk tag replacement cost only a cache miss.
/// </summary>
/// <param name="cacheInvalidator">Token source evicting all tags cache entries.</param>
public class PopularTagsCacheHandler(IPopularTagsCacheInvalidator cacheInvalidator)
    : IDomainEventHandler<TagGraphChangedEvent>
{
    /// <inheritdoc />
    public Task Handle(TagGraphChangedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        cacheInvalidator.Invalidate();
        return Task.CompletedTask;
    }
}
