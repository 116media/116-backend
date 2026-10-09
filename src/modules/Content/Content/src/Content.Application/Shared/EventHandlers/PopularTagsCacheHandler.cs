using _116.BuildingBlocks.Application.Services;
using _116.Content.Application.Shared.Cache;
using _116.Content.Domain.Events;
using Microsoft.Extensions.Caching.Hybrid;

namespace _116.Content.Application.Shared.EventHandlers;

/// <summary>
/// Evicts the tags caches whenever the tag graph changes, and whenever content is published
/// or unpublished: the listings carry per-kind content counts that count published content
/// only, so a publication moves numbers no tag event would report. The popular-tags and
/// all-tags projections share one cache tag, so a single eviction refreshes both. Eviction is
/// idempotent, so the several events raised by a bulk tag replacement cost only a cache miss.
/// </summary>
/// <param name="cache">The hybrid cache holding the tag projections.</param>
public class PopularTagsCacheHandler(HybridCache cache)
    : IDomainEventHandler<TagGraphChangedEvent>,
        IDomainEventHandler<ArticlePublishedEvent>,
        IDomainEventHandler<ArticleUnpublishedEvent>,
        IDomainEventHandler<VideoPublishedEvent>,
        IDomainEventHandler<VideoUnpublishedEvent>,
        IDomainEventHandler<CommissionedContentPublishedEvent>
{
    /// <inheritdoc />
    public async Task Handle(TagGraphChangedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await cache.RemoveByTagAsync(ContentCacheTags.Tags, cancellationToken);
    }

    /// <inheritdoc />
    public async Task Handle(ArticlePublishedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await cache.RemoveByTagAsync(ContentCacheTags.Tags, cancellationToken);
    }

    /// <inheritdoc />
    public async Task Handle(ArticleUnpublishedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await cache.RemoveByTagAsync(ContentCacheTags.Tags, cancellationToken);
    }

    /// <inheritdoc />
    public async Task Handle(VideoPublishedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await cache.RemoveByTagAsync(ContentCacheTags.Tags, cancellationToken);
    }

    /// <inheritdoc />
    public async Task Handle(VideoUnpublishedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await cache.RemoveByTagAsync(ContentCacheTags.Tags, cancellationToken);
    }

    /// <inheritdoc />
    public async Task Handle(
        CommissionedContentPublishedEvent domainEvent,
        CancellationToken cancellationToken = default
    )
    {
        await cache.RemoveByTagAsync(ContentCacheTags.Tags, cancellationToken);
    }
}
