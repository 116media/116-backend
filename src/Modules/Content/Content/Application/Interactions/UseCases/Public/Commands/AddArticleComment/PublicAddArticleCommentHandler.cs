using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.AddArticleComment;

/// <summary>
/// Handles the <see cref="PublicAddArticleCommentCommand" /> to post a comment on an article.
/// </summary>
/// <param name="articleCommentRepository">Repository for article comment data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class PublicAddArticleCommentHandler(
    IArticleCommentRepository articleCommentRepository,
    IContentUnitOfWork unitOfWork
) : ICommandHandler<PublicAddArticleCommentCommand, PublicAddArticleCommentResult>
{
    /// <inheritdoc />
    public async Task<PublicAddArticleCommentResult> Handle(
        PublicAddArticleCommentCommand command,
        CancellationToken cancellationToken
    )
    {
        await articleCommentRepository.ExistsOrThrowAsync(
            articleId: command.ArticleId,
            cancellationToken: cancellationToken
        );

        var comment = ArticleCommentEntity.Create(
            id: Guid.NewGuid(),
            userId: command.UserId,
            articleId: command.ArticleId,
            body: command.Body
        );

        await articleCommentRepository.AddCommentAsync(comment: comment, cancellationToken: cancellationToken);

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        var dto = comment.ToPublicArticleCommentDto();
        return new PublicAddArticleCommentResult(Comment: dto);
    }
}
