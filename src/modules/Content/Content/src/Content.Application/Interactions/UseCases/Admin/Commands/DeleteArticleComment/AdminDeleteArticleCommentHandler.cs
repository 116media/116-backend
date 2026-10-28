using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Interactions.UseCases.Admin.Commands.DeleteArticleComment;

/// <summary>
/// Handles the <see cref="AdminDeleteArticleCommentCommand" /> to soft-delete any article comment.
/// Deleting an already soft-deleted comment reports success without a write,
/// so moderating a comment the owner already removed never decrements the
/// article's cached comment count twice.
/// </summary>
/// <param name="articleCommentRepository">Repository for article comment data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="timeProvider">Clock stamping the deletion time.</param>
public class AdminDeleteArticleCommentHandler(
    IArticleCommentRepository articleCommentRepository,
    IContentUnitOfWork unitOfWork,
    TimeProvider timeProvider
) : ICommandHandler<AdminDeleteArticleCommentCommand, AdminDeleteArticleCommentResult>
{
    /// <inheritdoc />
    public async Task<AdminDeleteArticleCommentResult> Handle(
        AdminDeleteArticleCommentCommand command,
        CancellationToken cancellationToken = default
    )
    {
        ArticleCommentEntity? comment = await articleCommentRepository.GetCommentByIdAsync(
            commentId: command.CommentId,
            articleId: command.ArticleId,
            cancellationToken: cancellationToken
        );

        if (comment is null)
        {
            throw ArticleInteractionErrors.CommentNotFound(commentId: command.CommentId);
        }

        if (comment.SoftDelete(now: timeProvider.GetUtcNow()))
        {
            await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
        }

        return new AdminDeleteArticleCommentResult(IsSuccess: true);
    }
}
