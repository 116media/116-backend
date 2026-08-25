using _116.Content.Application.Shared.Cache;
using _116.Content.Domain.Events;
using _116.Shared.Application.Services;
using Microsoft.Extensions.Caching.Hybrid;

namespace _116.Content.Application.Shared.EventHandlers;

/// <summary>
/// Evicts the popular-articles and article-feed caches whenever an article's membership in the
/// published set changes: publication, departure from the published set, or
/// removal of the record. Engagement-driven eviction rides the engagement
/// handler that also moves the counter. Eviction is idempotent, so handling
/// the same fact more than once costs only a cache miss.
/// </summary>
/// <param name="cache">The hybrid cache holding the popular-articles feeds.</param>
public class PopularArticlesCacheHandler(HybridCache cache)
    : IDomainEventHandler<ArticlePublishedEvent>,
        IDomainEventHandler<ArticleUnpublishedEvent>,
        IDomainEventHandler<ArticleDeletedEvent>
{
    /// <inheritdoc />
    public async Task Handle(ArticlePublishedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await cache.RemoveByTagAsync(ContentCacheTags.PopularArticles, cancellationToken);
        await cache.RemoveByTagAsync(ContentCacheTags.Articles, cancellationToken);
    }

    /// <inheritdoc />
    public async Task Handle(ArticleUnpublishedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await cache.RemoveByTagAsync(ContentCacheTags.PopularArticles, cancellationToken);
        await cache.RemoveByTagAsync(ContentCacheTags.Articles, cancellationToken);
    }

    /// <inheritdoc />
    public async Task Handle(ArticleDeletedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await cache.RemoveByTagAsync(ContentCacheTags.PopularArticles, cancellationToken);
        await cache.RemoveByTagAsync(ContentCacheTags.Articles, cancellationToken);
    }
}
