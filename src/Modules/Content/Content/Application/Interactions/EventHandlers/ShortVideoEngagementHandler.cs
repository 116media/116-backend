using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Events;
using _116.Shared.Application.Services;
using Microsoft.Extensions.Logging;

namespace _116.Content.Application.Interactions.EventHandlers;

/// <summary>
/// Applies the denormalized engagement counters on short videos as
/// interaction rows are committed. No shorts cache exists, so the handler
/// performs no eviction; a future cache attaches to the same event with one
/// more handler. Runs post-commit in its own scope: the interaction row is
/// already durable and the rows remain the source of truth. A short video
/// that disappeared between the commit and the dispatch is skipped: the
/// counter dies with the row.
/// </summary>
/// <param name="shortVideoRepository">Repository for short video data access operations.</param>
/// <param name="logger">Logger recording events whose short video no longer exists.</param>
public class ShortVideoEngagementHandler(
    IShortVideoRepository shortVideoRepository,
    ILogger<ShortVideoEngagementHandler> logger
) : IDomainEventHandler<ShortVideoEngagedEvent>
{
    /// <inheritdoc />
    public async Task Handle(ShortVideoEngagedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        int? updated = await shortVideoRepository.ApplyEngagementDeltaAsync(
            shortVideoId: domainEvent.ShortVideoId,
            kind: domainEvent.Kind,
            delta: domainEvent.Delta,
            cancellationToken: cancellationToken
        );

        // null means this entity carries no counter for the kind, which is routine; 0 means the
        // row was deleted between the interaction commit and this post-commit dispatch.
        if (updated == 0)
        {
            logger.LogDebug(
                "Engagement counter skipped for short video {ShortVideoId}: the short video no longer exists.",
                domainEvent.ShortVideoId
            );
        }
    }
}
