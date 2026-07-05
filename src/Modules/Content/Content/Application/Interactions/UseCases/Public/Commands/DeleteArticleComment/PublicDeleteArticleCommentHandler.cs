using _116.Content.Application.Shared.Cache;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.DeleteArticleComment;

/// <summary>
/// Handles the <see cref="PublicDeleteArticleCommentCommand" /> to soft-delete a comment.
/// </summary>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="cacheInvalidator">Invalidates the popular-articles cache after the comment count changes.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class PublicDeleteArticleCommentHandler(
    IArticleRepository articleRepository,
    IContentUnitOfWork unitOfWork,
    IPopularArticlesCacheInvalidator cacheInvalidator,
    ContentI18n i18n
) : ICommandHandler<PublicDeleteArticleCommentCommand, PublicDeleteArticleCommentResult>
{
    /// <inheritdoc />
    public async Task<PublicDeleteArticleCommentResult> Handle(
        PublicDeleteArticleCommentCommand command,
        CancellationToken cancellationToken
    )
    {
        ArticleCommentEntity? comment = await articleRepository.GetCommentByIdAsync(
            commentId: command.CommentId,
            cancellationToken: cancellationToken
        );

        if (comment is not null)
        {
            if (comment.UserId != command.UserId)
            {
                throw i18n.ArticleInteraction.NotCommentOwner();
            }

            comment.SoftDelete();

            ArticleEntity article = await articleRepository.GetByIdOrThrowAsync(
                id: command.ArticleId,
                cancellationToken: cancellationToken
            );

            article.DecrementCommentCount();
            articleRepository.Update(article: article);
            articleRepository.UpdateComment(comment: comment);

            await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

            cacheInvalidator.Invalidate();

            return new PublicDeleteArticleCommentResult(IsSuccess: true);
        }

        throw i18n.ArticleInteraction.CommentNotFound(commentId: command.CommentId);
    }
}
