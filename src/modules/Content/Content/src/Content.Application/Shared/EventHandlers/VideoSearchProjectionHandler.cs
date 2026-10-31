using _116.BuildingBlocks.Application.Services;
using _116.Content.Application.Shared.Search;
using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;

namespace _116.Content.Application.Shared.EventHandlers;

/// <summary>
/// Keeps a video's search document in step with its publication state. A video's searchable
/// text is frozen once published, so these three transitions are the whole of what search
/// needs to hear. Projection is idempotent, so a replayed event is harmless.
/// </summary>
/// <param name="projection">The search projection write side.</param>
public class VideoSearchProjectionHandler(ISearchProjection projection)
    : IDomainEventHandler<VideoPublishedEvent>,
        IDomainEventHandler<VideoUnpublishedEvent>,
        IDomainEventHandler<VideoDeletedEvent>
{
    /// <inheritdoc />
    public Task Handle(VideoPublishedEvent domainEvent, CancellationToken cancellationToken = default) =>
        projection.ProjectAsync(EnumCoreContentType.Video, domainEvent.VideoId, cancellationToken);

    /// <inheritdoc />
    public Task Handle(VideoUnpublishedEvent domainEvent, CancellationToken cancellationToken = default) =>
        projection.ProjectAsync(EnumCoreContentType.Video, domainEvent.VideoId, cancellationToken);

    /// <inheritdoc />
    public Task Handle(VideoDeletedEvent domainEvent, CancellationToken cancellationToken = default) =>
        projection.RemoveAsync(EnumCoreContentType.Video, domainEvent.VideoId, cancellationToken);
}
