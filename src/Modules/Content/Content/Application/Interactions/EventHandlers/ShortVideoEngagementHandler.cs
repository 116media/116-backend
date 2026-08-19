using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
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
/// <param name="unitOfWork">Unit of Work committing the counter mutation.</param>
/// <param name="logger">Logger recording events whose short video no longer exists.</param>
public class ShortVideoEngagementHandler(
    IShortVideoRepository shortVideoRepository,
    IContentUnitOfWork unitOfWork,
    ILogger<ShortVideoEngagementHandler> logger
) : IDomainEventHandler<ShortVideoEngagedEvent>
{
    /// <inheritdoc />
    public async Task Handle(ShortVideoEngagedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        ShortVideoEntity? shortVideo = await shortVideoRepository.GetByIdAsync(
            id: domainEvent.ShortVideoId,
            cancellationToken: cancellationToken
        );

        if (shortVideo is null)
        {
            logger.LogDebug(
                "Engagement counter skipped for short video {ShortVideoId}: the short video no longer exists.",
                domainEvent.ShortVideoId
            );

            return;
        }

        switch (domainEvent.Kind, domainEvent.Delta)
        {
            case (EnumEngagementKind.Like, > 0):
                shortVideo.IncrementLikeCount();
                break;
            case (EnumEngagementKind.Like, < 0):
                shortVideo.DecrementLikeCount();
                break;
            case (EnumEngagementKind.Bookmark, > 0):
                shortVideo.IncrementBookmarkCount();
                break;
            case (EnumEngagementKind.Bookmark, < 0):
                shortVideo.DecrementBookmarkCount();
                break;
            case (EnumEngagementKind.Share, > 0):
                shortVideo.IncrementShareCount();
                break;
            case (EnumEngagementKind.View, > 0):
                shortVideo.IncrementViewCount();
                break;
            default:
                return;
        }

        shortVideoRepository.Update(shortVideo: shortVideo);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
    }
}
