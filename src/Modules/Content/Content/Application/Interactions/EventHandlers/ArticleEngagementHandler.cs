using _116.Content.Application.Shared.Cache;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
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
/// <param name="unitOfWork">Unit of Work committing the counter mutation.</param>
/// <param name="cacheInvalidator">Token source evicting all popular-articles cache entries.</param>
/// <param name="logger">Logger recording events whose article no longer exists.</param>
public class ArticleEngagementHandler(
    IArticleRepository articleRepository,
    IContentUnitOfWork unitOfWork,
    IPopularArticlesCacheInvalidator cacheInvalidator,
    ILogger<ArticleEngagementHandler> logger
) : IDomainEventHandler<ArticleEngagedEvent>
{
    /// <inheritdoc />
    public async Task Handle(ArticleEngagedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        ArticleEntity? article = await articleRepository.GetByIdAsync(
            id: domainEvent.ArticleId,
            cancellationToken: cancellationToken
        );

        if (article is null)
        {
            logger.LogDebug(
                "Engagement counter skipped for article {ArticleId}: the article no longer exists.",
                domainEvent.ArticleId
            );

            return;
        }

        switch (domainEvent.Kind, domainEvent.Delta)
        {
            case (EnumEngagementKind.Like, > 0):
                article.IncrementLikeCount();
                break;
            case (EnumEngagementKind.Like, < 0):
                article.DecrementLikeCount();
                break;
            case (EnumEngagementKind.Bookmark, > 0):
                article.IncrementBookmarkCount();
                break;
            case (EnumEngagementKind.Bookmark, < 0):
                article.DecrementBookmarkCount();
                break;
            case (EnumEngagementKind.Comment, > 0):
                article.IncrementCommentCount();
                break;
            case (EnumEngagementKind.Comment, < 0):
                article.DecrementCommentCount();
                break;
            case (EnumEngagementKind.Share, > 0):
                article.IncrementShareCount();
                break;
            default:
                cacheInvalidator.Invalidate();
                return;
        }

        articleRepository.Update(article: article);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        cacheInvalidator.Invalidate();
    }
}
