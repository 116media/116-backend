using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.EditArticleComment;

/// <summary>
/// Handles the <see cref="PublicEditArticleCommentCommand" /> to update a comment body.
/// </summary>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="articleInteractionErrors">Article interaction domain error factory.</param>
public class PublicEditArticleCommentHandler(
    IArticleRepository articleRepository,
    IContentUnitOfWork unitOfWork,
    ArticleInteractionErrors articleInteractionErrors
) : ICommandHandler<PublicEditArticleCommentCommand, PublicEditArticleCommentResult>
{
    /// <inheritdoc />
    public async Task<PublicEditArticleCommentResult> Handle(
        PublicEditArticleCommentCommand command,
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
                throw articleInteractionErrors.NotCommentOwner();
            }

            comment.Edit(body: command.Body);
            articleRepository.UpdateComment(comment: comment);

            await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

            return new PublicEditArticleCommentResult(IsSuccess: true);
        }

        throw articleInteractionErrors.CommentNotFound(commentId: command.CommentId);
    }
}
