using _116.Content.Application.Shared.Errors;
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
public class PublicUnbookmarkArticleHandler(IArticleRepository articleRepository, IContentUnitOfWork unitOfWork)
    : ICommandHandler<PublicUnbookmarkArticleCommand, PublicUnbookmarkArticleResult>
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
            throw ArticleInteractionErrors.BookmarkNotFound();
        }

        await articleRepository.RemoveBookmarkAsync(
            userId: command.UserId,
            articleId: command.ArticleId,
            cancellationToken: cancellationToken
        );

        article.DecrementBookmarkCount();
        articleRepository.Update(article: article);

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new PublicUnbookmarkArticleResult(IsSuccess: true);
    }
}
