using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Interactions.Services;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.AddCommentReply;

/// <summary>
/// Handles the <see cref="PublicAddCommentReplyCommand" /> to reply to a top-level article comment.
/// </summary>
/// <param name="articleCommentRepository">Repository resolving the parent and staging the reply.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="commentDtoService">Service assembling the reply with its author.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class PublicAddCommentReplyHandler(
    IArticleCommentRepository articleCommentRepository,
    IContentUnitOfWork unitOfWork,
    IArticleCommentDtoService commentDtoService,
    ContentI18n i18n
) : ICommandHandler<PublicAddCommentReplyCommand, PublicAddCommentReplyResult>
{
    /// <inheritdoc />
    public async Task<PublicAddCommentReplyResult> Handle(
        PublicAddCommentReplyCommand command,
        CancellationToken cancellationToken
    )
    {
        await articleCommentRepository.ExistsOrThrowAsync(
            articleId: command.ArticleId,
            cancellationToken: cancellationToken
        );

        ArticleCommentEntity? parent = await articleCommentRepository.GetCommentByIdAsync(
            commentId: command.ParentCommentId,
            cancellationToken: cancellationToken
        );

        if (parent is null || parent.ArticleId != command.ArticleId || parent.IsDeleted)
        {
            throw i18n.ArticleInteraction.CommentNotFound(command.ParentCommentId);
        }

        if (parent.ParentCommentId is not null)
        {
            throw i18n.ArticleInteraction.CannotReplyToReply();
        }

        var reply = ArticleCommentEntity.CreateReply(
            id: Guid.NewGuid(),
            userId: command.UserId,
            articleId: command.ArticleId,
            parentCommentId: command.ParentCommentId,
            body: command.Body
        );

        await articleCommentRepository.AddCommentAsync(comment: reply, cancellationToken: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        PublicArticleCommentDto dto = await commentDtoService.CreatePublicAsync(reply, cancellationToken);
        return new PublicAddCommentReplyResult(Reply: dto);
    }
}
