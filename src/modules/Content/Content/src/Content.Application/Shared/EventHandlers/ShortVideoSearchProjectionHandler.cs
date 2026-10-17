using _116.BuildingBlocks.Application.Services;
using _116.Content.Application.Shared.Search;
using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;

namespace _116.Content.Application.Shared.EventHandlers;

/// <summary>
/// Keeps a short video's search document in step. Shorts have no editorial workflow, so
/// their title stays mutable for life and every revision, activation and deactivation
/// raises the one change event this reprojects on.
/// </summary>
/// <param name="projection">The search projection write side.</param>
public class ShortVideoSearchProjectionHandler(ISearchProjection projection)
    : IDomainEventHandler<ShortVideoChangedEvent>,
        IDomainEventHandler<ShortVideoDeletedEvent>
{
    /// <inheritdoc />
    public Task Handle(ShortVideoChangedEvent domainEvent, CancellationToken cancellationToken = default) =>
        projection.ProjectAsync(EnumCoreContentType.Short, domainEvent.ShortVideoId, cancellationToken);

    /// <inheritdoc />
    public Task Handle(ShortVideoDeletedEvent domainEvent, CancellationToken cancellationToken = default) =>
        projection.RemoveAsync(EnumCoreContentType.Short, domainEvent.ShortVideoId, cancellationToken);
}
