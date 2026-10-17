using _116.BuildingBlocks.Application.Services;
using _116.Content.Application.Shared.Search;
using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;

namespace _116.Content.Application.Shared.EventHandlers;

/// <summary>
/// Keeps an article's search document in step with its publication state. An article's
/// searchable text is frozen once published, so these three transitions are the whole of
/// what search needs to hear. Projection is idempotent, so a replayed event is harmless.
/// </summary>
/// <param name="projection">The search projection write side.</param>
public class ArticleSearchProjectionHandler(ISearchProjection projection)
    : IDomainEventHandler<ArticlePublishedEvent>,
        IDomainEventHandler<ArticleUnpublishedEvent>,
        IDomainEventHandler<ArticleDeletedEvent>
{
    /// <inheritdoc />
    public Task Handle(ArticlePublishedEvent domainEvent, CancellationToken cancellationToken = default) =>
        projection.ProjectAsync(EnumCoreContentType.Article, domainEvent.ArticleId, cancellationToken);

    /// <inheritdoc />
    public Task Handle(ArticleUnpublishedEvent domainEvent, CancellationToken cancellationToken = default) =>
        projection.ProjectAsync(EnumCoreContentType.Article, domainEvent.ArticleId, cancellationToken);

    /// <inheritdoc />
    public Task Handle(ArticleDeletedEvent domainEvent, CancellationToken cancellationToken = default) =>
        projection.RemoveAsync(EnumCoreContentType.Article, domainEvent.ArticleId, cancellationToken);
}
