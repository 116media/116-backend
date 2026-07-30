using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using _116.Shared.Application.Services;
using Microsoft.Extensions.Logging;

namespace _116.Content.Application.Interactions.EventHandlers;

/// <summary>
/// Applies the denormalized engagement counters on lyrics pages as
/// interaction rows are committed. No popular-lyrics cache exists, so the
/// handler performs no eviction; a future cache attaches to the same event
/// with one more handler. Runs post-commit in its own scope: the
/// interaction row is already durable and the rows remain the source of
/// truth. A lyrics page that disappeared between the commit and the
/// dispatch is skipped: the counter dies with the row.
/// </summary>
/// <param name="lyricsRepository">Repository for lyrics data access operations.</param>
/// <param name="unitOfWork">Unit of Work committing the counter mutation.</param>
/// <param name="logger">Logger recording events whose lyrics page no longer exists.</param>
public class LyricsEngagementHandler(
    ILyricsRepository lyricsRepository,
    IContentUnitOfWork unitOfWork,
    ILogger<LyricsEngagementHandler> logger
) : IDomainEventHandler<LyricsEngagedEvent>
{
    /// <inheritdoc />
    public async Task Handle(LyricsEngagedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        LyricsEntity? lyrics = await lyricsRepository.GetByIdAsync(
            id: domainEvent.LyricsId,
            cancellationToken: cancellationToken
        );

        if (lyrics is null)
        {
            logger.LogDebug(
                "Engagement counter skipped for lyrics {LyricsId}: the lyrics page no longer exists.",
                domainEvent.LyricsId
            );

            return;
        }

        switch (domainEvent.Kind, domainEvent.Delta)
        {
            case (EnumEngagementKind.Like, > 0):
                lyrics.IncrementLikeCount();
                break;
            case (EnumEngagementKind.Like, < 0):
                lyrics.DecrementLikeCount();
                break;
            case (EnumEngagementKind.Share, > 0):
                lyrics.IncrementShareCount();
                break;
            case (EnumEngagementKind.View, > 0):
                lyrics.IncrementViewCount();
                break;
            default:
                return;
        }

        lyricsRepository.Update(lyrics: lyrics);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
    }
}
