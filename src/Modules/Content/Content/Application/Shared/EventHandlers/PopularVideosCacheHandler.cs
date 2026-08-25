using _116.Content.Application.Shared.Cache;
using _116.Content.Domain.Events;
using _116.Shared.Application.Services;
using Microsoft.Extensions.Caching.Hybrid;

namespace _116.Content.Application.Shared.EventHandlers;

/// <summary>
/// Evicts the popular-videos and video-feed caches whenever a video's membership in the
/// published set changes: publication, departure from the published set, or
/// removal of the record. Engagement-driven eviction rides the engagement
/// handler that also moves the counter. Eviction is idempotent, so handling
/// the same fact more than once costs only a cache miss.
/// </summary>
/// <param name="cache">The hybrid cache holding the popular-videos feeds.</param>
public class PopularVideosCacheHandler(HybridCache cache)
    : IDomainEventHandler<VideoPublishedEvent>,
        IDomainEventHandler<VideoUnpublishedEvent>,
        IDomainEventHandler<VideoDeletedEvent>
{
    /// <inheritdoc />
    public async Task Handle(VideoPublishedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await cache.RemoveByTagAsync(ContentCacheTags.PopularVideos, cancellationToken);
        await cache.RemoveByTagAsync(ContentCacheTags.Videos, cancellationToken);
    }

    /// <inheritdoc />
    public async Task Handle(VideoUnpublishedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await cache.RemoveByTagAsync(ContentCacheTags.PopularVideos, cancellationToken);
        await cache.RemoveByTagAsync(ContentCacheTags.Videos, cancellationToken);
    }

    /// <inheritdoc />
    public async Task Handle(VideoDeletedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await cache.RemoveByTagAsync(ContentCacheTags.PopularVideos, cancellationToken);
        await cache.RemoveByTagAsync(ContentCacheTags.Videos, cancellationToken);
    }
}
