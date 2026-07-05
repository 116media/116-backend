using _116.Content.Application.Shared.Cache;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.LikeArticle;

/// <summary>
/// Handles the <see cref="PublicLikeArticleCommand" /> to record a user's like on an article.
/// </summary>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="cacheInvalidator">Invalidates the popular-articles cache after the like count changes.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class PublicLikeArticleHandler(
    IArticleRepository articleRepository,
    IContentUnitOfWork unitOfWork,
    IPopularArticlesCacheInvalidator cacheInvalidator,
    ContentI18n i18n
) : ICommandHandler<PublicLikeArticleCommand, PublicLikeArticleResult>
{
    /// <inheritdoc />
    public async Task<PublicLikeArticleResult> Handle(
        PublicLikeArticleCommand command,
        CancellationToken cancellationToken
    )
    {
        ArticleEntity article = await articleRepository.GetByIdOrThrowAsync(
            id: command.ArticleId,
            cancellationToken: cancellationToken
        );

        bool alreadyLiked = await articleRepository.HasLikedAsync(
            userId: command.UserId,
            articleId: command.ArticleId,
            cancellationToken: cancellationToken
        );

        if (alreadyLiked)
        {
            throw i18n.ArticleInteraction.AlreadyLiked();
        }

        var like = ArticleLikeEntity.Create(id: Guid.NewGuid(), userId: command.UserId, articleId: command.ArticleId);

        await articleRepository.AddLikeAsync(like: like, cancellationToken: cancellationToken);

        article.IncrementLikeCount();
        articleRepository.Update(article: article);

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        cacheInvalidator.Invalidate();

        return new PublicLikeArticleResult(IsSuccess: true);
    }
}
