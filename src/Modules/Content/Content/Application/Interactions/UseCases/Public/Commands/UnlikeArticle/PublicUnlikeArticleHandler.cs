using _116.Content.Application.Shared.Cache;
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
/// <param name="cacheInvalidator">Invalidates the popular-articles cache after the like count changes.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class PublicUnlikeArticleHandler(
    IArticleRepository articleRepository,
    IContentUnitOfWork unitOfWork,
    IPopularArticlesCacheInvalidator cacheInvalidator,
    ContentI18n i18n
) : ICommandHandler<PublicUnlikeArticleCommand, PublicUnlikeArticleResult>
{
    /// <inheritdoc />
    public async Task<PublicUnlikeArticleResult> Handle(
        PublicUnlikeArticleCommand command,
        CancellationToken cancellationToken
    )
    {
        ArticleEntity article = await articleRepository.GetByIdOrThrowAsync(
            id: command.ArticleId,
            cancellationToken: cancellationToken
        );

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

        article.DecrementLikeCount();
        articleRepository.Update(article: article);

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        cacheInvalidator.Invalidate();

        return new PublicUnlikeArticleResult(IsSuccess: true);
    }
}
