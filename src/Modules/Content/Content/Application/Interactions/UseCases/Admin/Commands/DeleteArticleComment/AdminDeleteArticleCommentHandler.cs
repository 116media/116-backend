using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Admin.Commands.DeleteArticleComment;

/// <summary>
/// Handles the <see cref="AdminDeleteArticleCommentCommand" /> to soft-delete any article comment.
/// </summary>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="articleInteractionErrors">Article interaction domain error factory.</param>
public class AdminDeleteArticleCommentHandler(
    IArticleRepository articleRepository,
    IContentUnitOfWork unitOfWork,
    ArticleInteractionErrors articleInteractionErrors
) : ICommandHandler<AdminDeleteArticleCommentCommand, AdminDeleteArticleCommentResult>
{
    /// <inheritdoc />
    public async Task<AdminDeleteArticleCommentResult> Handle(
        AdminDeleteArticleCommentCommand command,
        CancellationToken cancellationToken
    )
    {
        ArticleCommentEntity? comment = await articleRepository.GetCommentByIdAsync(
            commentId: command.CommentId,
            cancellationToken: cancellationToken
        );

        if (comment is not null)
        {
            comment.SoftDelete();

            ArticleEntity article = await articleRepository.GetByIdOrThrowAsync(
                id: command.ArticleId,
                cancellationToken: cancellationToken
            );

            article.DecrementCommentCount();
            articleRepository.Update(article: article);
            articleRepository.UpdateComment(comment: comment);

            await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

            return new AdminDeleteArticleCommentResult(IsSuccess: true);
        }

        throw articleInteractionErrors.CommentNotFound(commentId: command.CommentId);
    }
}
