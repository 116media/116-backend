using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.UnlikeArticleComment;

/// <summary>
/// Handles the <see cref="PublicUnlikeArticleCommentCommand" /> to remove a user's like from a
/// comment and decrement its cached like count (never below zero). Idempotent: unliking a
/// comment the user has not liked makes no change and still reports success.
/// </summary>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class PublicUnlikeArticleCommentHandler(
    IArticleRepository articleRepository,
    IContentUnitOfWork unitOfWork,
    ContentI18n i18n
) : ICommandHandler<PublicUnlikeArticleCommentCommand, PublicUnlikeArticleCommentResult>
{
    /// <inheritdoc />
    public async Task<PublicUnlikeArticleCommentResult> Handle(
        PublicUnlikeArticleCommentCommand command,
        CancellationToken cancellationToken
    )
    {
        ArticleCommentEntity? comment = await articleRepository.GetCommentByIdAsync(
            commentId: command.CommentId,
            cancellationToken: cancellationToken
        );

        if (comment is null || comment.IsDeleted)
        {
            throw i18n.ArticleInteraction.CommentNotFound(command.CommentId);
        }

        bool hasLiked = await articleRepository.HasLikedCommentAsync(
            userId: command.UserId,
            commentId: command.CommentId,
            cancellationToken: cancellationToken
        );

        if (!hasLiked)
        {
            return new PublicUnlikeArticleCommentResult(IsSuccess: true);
        }

        await articleRepository.RemoveCommentLikeAsync(
            userId: command.UserId,
            commentId: command.CommentId,
            cancellationToken: cancellationToken
        );

        comment.DecrementLikeCount();
        articleRepository.UpdateComment(comment: comment);

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new PublicUnlikeArticleCommentResult(IsSuccess: true);
    }
}
