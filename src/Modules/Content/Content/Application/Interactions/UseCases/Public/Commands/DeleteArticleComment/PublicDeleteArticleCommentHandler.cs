using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.DeleteArticleComment;

/// <summary>
/// Handles the <see cref="PublicDeleteArticleCommentCommand" /> to soft-delete a comment.
/// Deleting an already soft-deleted comment reports success without a write,
/// so a repeated delete never decrements the article's cached comment count
/// twice.
/// </summary>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class PublicDeleteArticleCommentHandler(
    IArticleRepository articleRepository,
    IContentUnitOfWork unitOfWork,
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
            articleId: command.ArticleId,
            cancellationToken: cancellationToken
        );

        if (comment is null)
        {
            throw i18n.ArticleInteraction.CommentNotFound(commentId: command.CommentId);
        }

        if (comment.UserId != command.UserId)
        {
            throw i18n.ArticleInteraction.NotCommentOwner();
        }

        if (comment.SoftDelete())
        {
            articleRepository.UpdateComment(comment: comment);
            await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
        }

        return new PublicDeleteArticleCommentResult(IsSuccess: true);
    }
}
