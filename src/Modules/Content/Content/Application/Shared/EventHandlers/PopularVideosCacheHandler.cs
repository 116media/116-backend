using _116.Content.Application.Shared.Cache;
using _116.Content.Domain.Events;
using _116.Shared.Application.Services;

namespace _116.Content.Application.Shared.EventHandlers;

/// <summary>
/// Evicts the popular-videos cache whenever a video's membership in the
/// published set changes: publication, departure from the published set, or
/// removal of the record. Engagement-driven eviction rides the engagement
/// handler that also moves the counter. Eviction is idempotent, so handling
/// the same fact more than once costs only a cache miss.
/// </summary>
/// <param name="cacheInvalidator">Token source evicting all popular-videos cache entries.</param>
public class PopularVideosCacheHandler(IPopularVideosCacheInvalidator cacheInvalidator)
    : IDomainEventHandler<VideoPublishedEvent>,
        IDomainEventHandler<VideoUnpublishedEvent>,
        IDomainEventHandler<VideoDeletedEvent>
{
    /// <inheritdoc />
    public Task Handle(VideoPublishedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        cacheInvalidator.Invalidate();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task Handle(VideoUnpublishedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        cacheInvalidator.Invalidate();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task Handle(VideoDeletedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        cacheInvalidator.Invalidate();
        return Task.CompletedTask;
    }
}
