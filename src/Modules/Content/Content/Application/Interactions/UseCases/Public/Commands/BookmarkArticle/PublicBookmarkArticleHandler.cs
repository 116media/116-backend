using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.BookmarkArticle;

/// <summary>
/// Handles the <see cref="PublicBookmarkArticleCommand" /> to record a user's bookmark on an article.
/// </summary>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class PublicBookmarkArticleHandler(IArticleRepository articleRepository, IContentUnitOfWork unitOfWork)
    : ICommandHandler<PublicBookmarkArticleCommand, PublicBookmarkArticleResult>
{
    /// <inheritdoc />
    public async Task<PublicBookmarkArticleResult> Handle(
        PublicBookmarkArticleCommand command,
        CancellationToken cancellationToken
    )
    {
        ArticleEntity article = await articleRepository.GetByIdOrThrowAsync(
            id: command.ArticleId,
            cancellationToken: cancellationToken
        );

        bool alreadyBookmarked = await articleRepository.HasBookmarkedAsync(
            userId: command.UserId,
            articleId: command.ArticleId,
            cancellationToken: cancellationToken
        );

        if (alreadyBookmarked)
        {
            throw ArticleInteractionErrors.AlreadyBookmarked();
        }

        var bookmark = ArticleBookmarkEntity.Create(
            id: Guid.NewGuid(),
            userId: command.UserId,
            articleId: command.ArticleId
        );

        await articleRepository.AddBookmarkAsync(bookmark: bookmark, cancellationToken: cancellationToken);

        article.IncrementBookmarkCount();
        articleRepository.Update(article: article);

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new PublicBookmarkArticleResult(IsSuccess: true);
    }
}
