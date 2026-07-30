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
/// <param name="unitOfWork">Unit of Work committing the counter mutation.</param>
/// <param name="cacheInvalidator">Token source evicting all popular-videos cache entries.</param>
/// <param name="logger">Logger recording events whose video no longer exists.</param>
public class VideoEngagementHandler(
    IVideoRepository videoRepository,
    IContentUnitOfWork unitOfWork,
    IPopularVideosCacheInvalidator cacheInvalidator,
    ILogger<VideoEngagementHandler> logger
) : IDomainEventHandler<VideoEngagedEvent>
{
    /// <inheritdoc />
    public async Task Handle(VideoEngagedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        VideoEntity? video = await videoRepository.GetByIdAsync(
            id: domainEvent.VideoId,
            cancellationToken: cancellationToken
        );

        if (video is null)
        {
            logger.LogDebug(
                "Engagement counter skipped for video {VideoId}: the video no longer exists.",
                domainEvent.VideoId
            );

            return;
        }

        switch (domainEvent.Kind, domainEvent.Delta)
        {
            case (EnumEngagementKind.Share, > 0):
                video.IncrementShareCount();
                break;
            case (EnumEngagementKind.Rating, _):
                await RecomputeRatingAsync(video: video, cancellationToken: cancellationToken);
                break;
            default:
                cacheInvalidator.Invalidate();
                return;
        }

        videoRepository.Update(video: video);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        cacheInvalidator.Invalidate();
    }

    /// <summary>
    /// Recomputes the video's cached rating average and count from the
    /// committed rating rows. Reading the rows post-commit guarantees the
    /// freshly created or restarred rating is included.
    /// </summary>
    /// <param name="video">The video whose rating aggregates are recomputed.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    private async Task RecomputeRatingAsync(VideoEntity video, CancellationToken cancellationToken)
    {
        List<VideoRatingEntity> allRatings = await videoRepository.GetAllRatingsForVideoAsync(
            videoId: video.Id,
            cancellationToken: cancellationToken
        );

        int count = allRatings.Count;
        decimal average = count > 0 ? (decimal)allRatings.Sum(r => r.Stars) / count : 0m;

        video.UpdateRating(average: Math.Round(average, 2), count: count);
    }
}
