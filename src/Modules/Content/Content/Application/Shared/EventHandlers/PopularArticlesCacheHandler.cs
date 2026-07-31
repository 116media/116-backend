using _116.Content.Application.Shared.Cache;
using _116.Content.Domain.Events;
using _116.Shared.Application.Services;

namespace _116.Content.Application.Shared.EventHandlers;

/// <summary>
/// Evicts the popular-articles cache whenever an article's membership in the
/// published set changes: publication, departure from the published set, or
/// removal of the record. Engagement-driven eviction rides the engagement
/// handler that also moves the counter. Eviction is idempotent, so handling
/// the same fact more than once costs only a cache miss.
/// </summary>
/// <param name="cacheInvalidator">Token source evicting all popular-articles cache entries.</param>
public class PopularArticlesCacheHandler(IPopularArticlesCacheInvalidator cacheInvalidator)
    : IDomainEventHandler<ArticlePublishedEvent>,
        IDomainEventHandler<ArticleUnpublishedEvent>,
        IDomainEventHandler<ArticleDeletedEvent>
{
    /// <inheritdoc />
    public Task Handle(ArticlePublishedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        cacheInvalidator.Invalidate();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task Handle(ArticleUnpublishedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        cacheInvalidator.Invalidate();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task Handle(ArticleDeletedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        cacheInvalidator.Invalidate();
        return Task.CompletedTask;
    }
}
