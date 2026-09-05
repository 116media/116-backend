using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Events;
using _116.Shared.Application.Services;
using Microsoft.Extensions.Logging;

namespace _116.Content.Application.Interactions.EventHandlers;

/// <summary>
/// Applies the comment-local cached like counter as comment-like rows are
/// committed. The counter lives on the comment itself, so no content-level
/// cache is evicted. A comment that disappeared between the commit and the
/// dispatch is skipped: the counter dies with the row. Runs post-commit in
/// its own scope: the like row is already durable and the rows remain the
/// source of truth.
/// </summary>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="logger">Logger recording events whose comment no longer exists.</param>
public class CommentEngagementHandler(IArticleRepository articleRepository, ILogger<CommentEngagementHandler> logger)
    : IDomainEventHandler<CommentEngagedEvent>
{
    /// <inheritdoc />
    public async Task Handle(CommentEngagedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        int updated = await articleRepository.ApplyCommentLikeDeltaAsync(
            commentId: domainEvent.CommentId,
            delta: domainEvent.Delta,
            cancellationToken: cancellationToken
        );

        if (updated == 0)
        {
            logger.LogDebug(
                "Engagement counter skipped for comment {CommentId}: the comment no longer exists.",
                domainEvent.CommentId
            );
        }
    }
}
