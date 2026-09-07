using _116.Content.Application.Shared.Cache;
using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using _116.Shared.Application.Services;
using Microsoft.Extensions.Caching.Hybrid;

namespace _116.Content.Application.Shared.EventHandlers;

/// <summary>
/// Evicts the shorts, artist and lyrics feed caches, plus the per-type feed on promotion and
/// commissioned-content transitions. Eviction is idempotent, so handling the same fact more
/// than once costs only a cache miss.
/// </summary>
/// <param name="cache">The hybrid cache holding the feed projections.</param>
public class ContentFeedCacheHandler(HybridCache cache)
    : IDomainEventHandler<ShortVideoChangedEvent>,
        IDomainEventHandler<ShortVideoDeletedEvent>,
        IDomainEventHandler<ArtistChangedEvent>,
        IDomainEventHandler<ArtistOwnershipVerifiedEvent>,
        IDomainEventHandler<LyricsRevisionDecidedEvent>,
        IDomainEventHandler<TranslationRevisionDecidedEvent>,
        IDomainEventHandler<CommissionedContentPublishedEvent>,
        IDomainEventHandler<CommissionedContentRejectedEvent>,
        IDomainEventHandler<ContentPromotionRemovedEvent>,
        IDomainEventHandler<OrderPaidEvent>
{
    /// <inheritdoc />
    public async Task Handle(ShortVideoChangedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await cache.RemoveByTagAsync(ContentCacheTags.Shorts, cancellationToken);
    }

    /// <inheritdoc />
    public async Task Handle(ShortVideoDeletedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await cache.RemoveByTagAsync(ContentCacheTags.Shorts, cancellationToken);
    }

    /// <inheritdoc />
    public async Task Handle(ArtistChangedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await cache.RemoveByTagAsync(ContentCacheTags.Artists, cancellationToken);
    }

    /// <inheritdoc />
    public async Task Handle(ArtistOwnershipVerifiedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await cache.RemoveByTagAsync(ContentCacheTags.Artists, cancellationToken);
    }

    /// <inheritdoc />
    public async Task Handle(LyricsRevisionDecidedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await cache.RemoveByTagAsync(ContentCacheTags.Lyrics, cancellationToken);
    }

    /// <inheritdoc />
    public async Task Handle(TranslationRevisionDecidedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await cache.RemoveByTagAsync(ContentCacheTags.Lyrics, cancellationToken);
    }

    /// <inheritdoc />
    public async Task Handle(
        CommissionedContentPublishedEvent domainEvent,
        CancellationToken cancellationToken = default
    )
    {
        await cache.RemoveByTagAsync(TagFor(domainEvent.ContentType), cancellationToken);
    }

    /// <inheritdoc />
    public async Task Handle(
        CommissionedContentRejectedEvent domainEvent,
        CancellationToken cancellationToken = default
    )
    {
        await cache.RemoveByTagAsync(TagFor(domainEvent.ContentType), cancellationToken);
    }

    /// <inheritdoc />
    public async Task Handle(ContentPromotionRemovedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await cache.RemoveByTagAsync(TagFor(domainEvent.ContentType), cancellationToken);
    }

    /// <inheritdoc />
    /// <remarks>
    /// A paid order grants promotions whose target types are not carried on the event, so every
    /// promotable feed is evicted.
    /// </remarks>
    public async Task Handle(OrderPaidEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await cache.RemoveByTagAsync(ContentCacheTags.Articles, cancellationToken);
        await cache.RemoveByTagAsync(ContentCacheTags.Videos, cancellationToken);
        await cache.RemoveByTagAsync(ContentCacheTags.Lyrics, cancellationToken);
    }

    /// <summary>
    /// Maps a content type to the feed tag its cached projections live under.
    /// </summary>
    private static string TagFor(EnumCoreContentType contentType)
    {
        return contentType switch
        {
            EnumCoreContentType.Article => ContentCacheTags.Articles,
            EnumCoreContentType.Video => ContentCacheTags.Videos,
            EnumCoreContentType.Lyrics => ContentCacheTags.Lyrics,
            _ => ContentCacheTags.Articles,
        };
    }
}
