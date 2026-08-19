using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.LikeArticleComment;

/// <summary>
/// Handles the <see cref="PublicLikeArticleCommentCommand" /> to record a user's like on a
/// comment and bump its cached like count. Idempotent: a like on an already-liked comment
/// makes no change and still reports success.
/// </summary>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class PublicLikeArticleCommentHandler(
    IArticleRepository articleRepository,
    IContentUnitOfWork unitOfWork,
    ContentI18n i18n
) : ICommandHandler<PublicLikeArticleCommentCommand, PublicLikeArticleCommentResult>
{
    /// <inheritdoc />
    public async Task<PublicLikeArticleCommentResult> Handle(
        PublicLikeArticleCommentCommand command,
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

        bool alreadyLiked = await articleRepository.HasLikedCommentAsync(
            userId: command.UserId,
            commentId: command.CommentId,
            cancellationToken: cancellationToken
        );

        if (alreadyLiked)
        {
            return new PublicLikeArticleCommentResult(IsSuccess: true);
        }

        var like = ArticleCommentLikeEntity.Create(
            id: Guid.NewGuid(),
            userId: command.UserId,
            commentId: command.CommentId
        );

        await articleRepository.AddCommentLikeAsync(like: like, cancellationToken: cancellationToken);

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new PublicLikeArticleCommentResult(IsSuccess: true);
    }
}
