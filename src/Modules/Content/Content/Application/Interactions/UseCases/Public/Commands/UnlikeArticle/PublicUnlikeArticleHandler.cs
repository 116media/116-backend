using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.UnlikeArticle;

/// <summary>
/// Handles the <see cref="PublicUnlikeArticleCommand" /> to remove a user's like from an article.
/// </summary>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class PublicUnlikeArticleHandler(
    IArticleRepository articleRepository,
    IContentUnitOfWork unitOfWork,
    ContentI18n i18n
) : ICommandHandler<PublicUnlikeArticleCommand, PublicUnlikeArticleResult>
{
    /// <inheritdoc />
    public async Task<PublicUnlikeArticleResult> Handle(
        PublicUnlikeArticleCommand command,
        CancellationToken cancellationToken
    )
    {
        await articleRepository.GetByIdOrThrowAsync(id: command.ArticleId, cancellationToken: cancellationToken);

        bool hasLiked = await articleRepository.HasLikedAsync(
            userId: command.UserId,
            articleId: command.ArticleId,
            cancellationToken: cancellationToken
        );

        if (!hasLiked)
        {
            throw i18n.ArticleInteraction.LikeNotFound();
        }

        await articleRepository.RemoveLikeAsync(
            userId: command.UserId,
            articleId: command.ArticleId,
            cancellationToken: cancellationToken
        );

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new PublicUnlikeArticleResult(IsSuccess: true);
    }
}
