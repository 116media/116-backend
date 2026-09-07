using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.ShareArticle;

/// <summary>
/// Handles the <see cref="PublicShareArticleCommand" /> to record a share event on an article.
/// </summary>
/// <param name="articleInteractionRepository">Repository for article interaction data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class PublicShareArticleHandler(
    IArticleInteractionRepository articleInteractionRepository,
    IContentUnitOfWork unitOfWork
) : ICommandHandler<PublicShareArticleCommand, PublicShareArticleResult>
{
    /// <inheritdoc />
    public async Task<PublicShareArticleResult> Handle(
        PublicShareArticleCommand command,
        CancellationToken cancellationToken
    )
    {
        await articleInteractionRepository.ExistsOrThrowAsync(
            articleId: command.ArticleId,
            cancellationToken: cancellationToken
        );

        var share = ArticleShareEntity.Create(
            id: Guid.NewGuid(),
            userId: command.UserId,
            articleId: command.ArticleId,
            shareChannel: command.ShareChannel
        );

        await articleInteractionRepository.AddShareAsync(share: share, cancellationToken: cancellationToken);

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new PublicShareArticleResult(IsSuccess: true);
    }
}
