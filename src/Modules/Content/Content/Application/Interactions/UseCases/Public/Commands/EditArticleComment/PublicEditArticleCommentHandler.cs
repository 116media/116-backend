using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.EditArticleComment;

/// <summary>
/// Handles the <see cref="PublicEditArticleCommentCommand" /> to update a comment body.
/// The comment is looked up scoped to the article in the route, so a comment reached
/// through a different article's id is never edited.
/// </summary>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class PublicEditArticleCommentHandler(
    IArticleRepository articleRepository,
    IContentUnitOfWork unitOfWork,
    ContentI18n i18n
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

        comment.Edit(body: command.Body);
        articleRepository.UpdateComment(comment: comment);

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new PublicEditArticleCommentResult(IsSuccess: true);
    }
}
