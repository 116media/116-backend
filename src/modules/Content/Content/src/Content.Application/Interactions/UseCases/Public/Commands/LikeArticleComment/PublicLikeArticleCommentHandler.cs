using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.LikeArticleComment;

/// <summary>
/// Handles the <see cref="PublicLikeArticleCommentCommand" /> to record a user's like on a
/// comment and bump its cached like count. Idempotent: a like on an already-liked comment
/// makes no change and still reports success.
/// </summary>
/// <param name="articleCommentRepository">Repository for article comment data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class PublicLikeArticleCommentHandler(
    IArticleCommentRepository articleCommentRepository,
    IContentUnitOfWork unitOfWork
) : ICommandHandler<PublicLikeArticleCommentCommand, PublicLikeArticleCommentResult>
{
    /// <inheritdoc />
    public async Task<PublicLikeArticleCommentResult> Handle(
        PublicLikeArticleCommentCommand command,
        CancellationToken cancellationToken = default
    )
    {
        ArticleCommentEntity? comment = await articleCommentRepository.GetCommentByIdAsync(
            commentId: command.CommentId,
            cancellationToken: cancellationToken
        );

        if (comment is null || comment.IsDeleted)
        {
            throw ArticleInteractionErrors.CommentNotFound(command.CommentId);
        }

        bool alreadyLiked = await articleCommentRepository.HasLikedCommentAsync(
            userId: command.UserId,
            commentId: command.CommentId,
            cancellationToken: cancellationToken
        );

        if (alreadyLiked)
        {
            return new PublicLikeArticleCommentResult(IsSuccess: true);
        }

        var like = ArticleCommentLikeEntity.Create(
            id: Guid.NewGuid(),
            userId: command.UserId,
            commentId: command.CommentId
        );

        await articleCommentRepository.AddCommentLikeAsync(like: like, cancellationToken: cancellationToken);

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new PublicLikeArticleCommentResult(IsSuccess: true);
    }
}
