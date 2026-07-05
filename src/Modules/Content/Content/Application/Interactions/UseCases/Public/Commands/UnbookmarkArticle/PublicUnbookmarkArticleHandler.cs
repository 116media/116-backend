using _116.Content.Application.Shared.Cache;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.UnbookmarkArticle;

/// <summary>
/// Handles the <see cref="PublicUnbookmarkArticleCommand" /> to remove a user's bookmark from an article.
/// </summary>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="cacheInvalidator">Invalidates the popular-articles cache after the bookmark count changes.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class PublicUnbookmarkArticleHandler(
    IArticleRepository articleRepository,
    IContentUnitOfWork unitOfWork,
    IPopularArticlesCacheInvalidator cacheInvalidator,
    ContentI18n i18n
) : ICommandHandler<PublicUnbookmarkArticleCommand, PublicUnbookmarkArticleResult>
{
    /// <inheritdoc />
    public async Task<PublicUnbookmarkArticleResult> Handle(
        PublicUnbookmarkArticleCommand command,
        CancellationToken cancellationToken
    )
    {
        ArticleEntity article = await articleRepository.GetByIdOrThrowAsync(
            id: command.ArticleId,
            cancellationToken: cancellationToken
        );

        bool hasBookmarked = await articleRepository.HasBookmarkedAsync(
            userId: command.UserId,
            articleId: command.ArticleId,
            cancellationToken: cancellationToken
        );

        if (!hasBookmarked)
        {
            throw i18n.ArticleInteraction.BookmarkNotFound();
        }

        await articleRepository.RemoveBookmarkAsync(
            userId: command.UserId,
            articleId: command.ArticleId,
            cancellationToken: cancellationToken
        );

        article.DecrementBookmarkCount();
        articleRepository.Update(article: article);

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        cacheInvalidator.Invalidate();

        return new PublicUnbookmarkArticleResult(IsSuccess: true);
    }
}
