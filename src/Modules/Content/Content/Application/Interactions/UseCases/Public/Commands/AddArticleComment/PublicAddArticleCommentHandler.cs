using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.AddArticleComment;

/// <summary>
/// Handles the <see cref="PublicAddArticleCommentCommand" /> to post a comment on an article.
/// </summary>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">The mapper used to project entities to DTOs.</param>
public class PublicAddArticleCommentHandler(
    IArticleRepository articleRepository,
    IContentUnitOfWork unitOfWork,
    IMapper mapper
) : ICommandHandler<PublicAddArticleCommentCommand, PublicAddArticleCommentResult>
{
    /// <inheritdoc />
    public async Task<PublicAddArticleCommentResult> Handle(
        PublicAddArticleCommentCommand command,
        CancellationToken cancellationToken
    )
    {
        ArticleEntity article = await articleRepository.GetByIdOrThrowAsync(
            id: command.ArticleId,
            cancellationToken: cancellationToken
        );

        var comment = ArticleCommentEntity.Create(
            id: Guid.NewGuid(),
            userId: command.UserId,
            articleId: command.ArticleId,
            body: command.Body
        );

        await articleRepository.AddCommentAsync(comment: comment, cancellationToken: cancellationToken);

        article.IncrementCommentCount();
        articleRepository.Update(article: article);

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        var dto = comment.ToArticleCommentDto(mapper);
        return new PublicAddArticleCommentResult(Comment: dto);
    }
}
