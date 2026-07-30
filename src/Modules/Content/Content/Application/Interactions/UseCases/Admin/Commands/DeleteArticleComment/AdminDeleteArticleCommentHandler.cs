using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Admin.Commands.DeleteArticleComment;

/// <summary>
/// Handles the <see cref="AdminDeleteArticleCommentCommand" /> to soft-delete any article comment.
/// Deleting an already soft-deleted comment reports success without a write,
/// so moderating a comment the owner already removed never decrements the
/// article's cached comment count twice.
/// </summary>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminDeleteArticleCommentHandler(
    IArticleRepository articleRepository,
    IContentUnitOfWork unitOfWork,
    ContentI18n i18n
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
            await articleRepository.GetByIdOrThrowAsync(id: command.ArticleId, cancellationToken: cancellationToken);

            if (comment.SoftDelete())
            {
                articleRepository.UpdateComment(comment: comment);
                await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
            }

            return new AdminDeleteArticleCommentResult(IsSuccess: true);
        }

        throw i18n.ArticleInteraction.CommentNotFound(commentId: command.CommentId);
    }
}
