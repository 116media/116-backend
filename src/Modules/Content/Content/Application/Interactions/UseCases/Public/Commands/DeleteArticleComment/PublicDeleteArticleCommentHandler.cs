using _116.Content.Application.Shared.Errors;
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
public class PublicDeleteArticleCommentHandler(IArticleRepository articleRepository, IContentUnitOfWork unitOfWork)
    : ICommandHandler<PublicDeleteArticleCommentCommand, PublicDeleteArticleCommentResult>
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

        if (comment is null)
        {
            throw ArticleInteractionErrors.CommentNotFound(commentId: command.CommentId);
        }

        if (comment.UserId != command.UserId)
        {
            throw ArticleInteractionErrors.NotCommentOwner();
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

        return new PublicDeleteArticleCommentResult(IsSuccess: true);
    }
}
