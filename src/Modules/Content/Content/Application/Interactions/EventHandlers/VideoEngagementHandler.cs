using _116.Content.Application.Shared.Cache;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using _116.Shared.Application.Services;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace _116.Content.Application.Interactions.EventHandlers;

/// <summary>
/// Applies the denormalized engagement state on videos as interaction rows
/// are committed, then evicts the popular-videos cache so the ranked list
/// reflects the change. Shares bump the cached share counter; ratings
/// recompute the cached average and count from the committed rating rows,
/// so the event's delta is never trusted for ratings. Runs post-commit in
/// its own scope: the interaction row is already durable and the rows
/// remain the source of truth. A video that disappeared between the commit
/// and the dispatch is skipped: the counter dies with the row.
/// </summary>
/// <param name="videoRepository">Repository for video data access operations.</param>
/// <param name="cache">The hybrid cache holding the popular-videos feeds.</param>
/// <param name="logger">Logger recording events whose video no longer exists.</param>
public class VideoEngagementHandler(
    IVideoRepository videoRepository,
    HybridCache cache,
    ILogger<VideoEngagementHandler> logger
) : IDomainEventHandler<VideoEngagedEvent>
{
    /// <inheritdoc />
    public async Task Handle(VideoEngagedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        // A rating is a recomputed average, never a delta, so it takes its own set-based write.
        int? updated =
            domainEvent.Kind == EnumEngagementKind.Rating
                ? await RefreshRatingAsync(videoId: domainEvent.VideoId, cancellationToken: cancellationToken)
                : await videoRepository.ApplyEngagementDeltaAsync(
                    videoId: domainEvent.VideoId,
                    kind: domainEvent.Kind,
                    delta: domainEvent.Delta,
                    cancellationToken: cancellationToken
                );

        // null means this entity carries no counter for the kind, which is routine; 0 means the
        // row was deleted between the interaction commit and this post-commit dispatch.
        if (updated == 0)
        {
            logger.LogDebug(
                "Engagement counter skipped for video {VideoId}: the video no longer exists.",
                domainEvent.VideoId
            );
        }

        await cache.RemoveByTagAsync(ContentCacheTags.PopularVideos, cancellationToken);
    }

    /// <summary>
    /// Recomputes the cached rating from the committed rating rows and writes it set-based.
    /// The event's delta is never trusted for ratings.
    /// </summary>
    /// <param name="videoId">The video whose rating is refreshed.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>Rows updated; <c>0</c> when the video no longer exists.</returns>
    private async Task<int> RefreshRatingAsync(Guid videoId, CancellationToken cancellationToken)
    {
        List<VideoRatingEntity> allRatings = await videoRepository.GetAllRatingsForVideoAsync(
            videoId: videoId,
            cancellationToken: cancellationToken
        );

        int count = allRatings.Count;
        decimal average = count > 0 ? (decimal)allRatings.Sum(r => r.Stars) / count : 0m;

        return await videoRepository.SetRatingAsync(
            videoId: videoId,
            average: Math.Round(average, 2),
            count: count,
            cancellationToken: cancellationToken
        );
    }
}
