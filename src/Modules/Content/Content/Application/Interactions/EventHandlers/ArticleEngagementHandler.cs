using _116.Content.Application.Shared.Cache;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Events;
using _116.Shared.Application.Services;
using Microsoft.Extensions.Logging;

namespace _116.Content.Application.Interactions.EventHandlers;

/// <summary>
/// Applies the denormalized engagement counters on articles as interaction
/// rows are committed, then evicts the popular-articles cache so the ranked
/// list reflects the new score. Runs post-commit in its own scope: the
/// interaction row is already durable, and the counter is the cached
/// approximation the module documents it to be — the rows remain the source
/// of truth. An article that disappeared between the commit and the dispatch
/// is skipped: the counter dies with the row.
/// </summary>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="cacheInvalidator">Token source evicting all popular-articles cache entries.</param>
/// <param name="logger">Logger recording events whose article no longer exists.</param>
public class ArticleEngagementHandler(
    IArticleRepository articleRepository,
    IPopularArticlesCacheInvalidator cacheInvalidator,
    ILogger<ArticleEngagementHandler> logger
) : IDomainEventHandler<ArticleEngagedEvent>
{
    /// <inheritdoc />
    public async Task Handle(ArticleEngagedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        int? updated = await articleRepository.ApplyEngagementDeltaAsync(
            articleId: domainEvent.ArticleId,
            kind: domainEvent.Kind,
            delta: domainEvent.Delta,
            cancellationToken: cancellationToken
        );

        // null means this entity carries no counter for the kind, which is routine; 0 means the
        // row was deleted between the interaction commit and this post-commit dispatch.
        if (updated == 0)
        {
            logger.LogDebug(
                "Engagement counter skipped for article {ArticleId}: the article no longer exists.",
                domainEvent.ArticleId
            );
        }

        cacheInvalidator.Invalidate();
    }
}
