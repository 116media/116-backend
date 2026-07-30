using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
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
/// <param name="unitOfWork">Unit of Work committing the counter mutation.</param>
/// <param name="logger">Logger recording events whose comment no longer exists.</param>
public class CommentEngagementHandler(
    IArticleRepository articleRepository,
    IContentUnitOfWork unitOfWork,
    ILogger<CommentEngagementHandler> logger
) : IDomainEventHandler<CommentEngagedEvent>
{
    /// <inheritdoc />
    public async Task Handle(CommentEngagedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        ArticleCommentEntity? comment = await articleRepository.GetCommentByIdAsync(
            commentId: domainEvent.CommentId,
            cancellationToken: cancellationToken
        );

        if (comment is null)
        {
            logger.LogDebug(
                "Engagement counter skipped for comment {CommentId}: the comment no longer exists.",
                domainEvent.CommentId
            );

            return;
        }

        if (domainEvent.Delta > 0)
        {
            comment.IncrementLikeCount();
        }
        else
        {
            comment.DecrementLikeCount();
        }

        articleRepository.UpdateComment(comment: comment);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
    }
}
