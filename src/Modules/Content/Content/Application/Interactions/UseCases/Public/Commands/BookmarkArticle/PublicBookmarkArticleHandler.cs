using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.BookmarkArticle;

/// <summary>
/// Handles the <see cref="PublicBookmarkArticleCommand" /> to record a user's bookmark on an article.
/// </summary>
/// <param name="articleInteractionRepository">Repository for article interaction data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class PublicBookmarkArticleHandler(
    IArticleInteractionRepository articleInteractionRepository,
    IContentUnitOfWork unitOfWork,
    ContentI18n i18n
) : ICommandHandler<PublicBookmarkArticleCommand, PublicBookmarkArticleResult>
{
    /// <inheritdoc />
    public async Task<PublicBookmarkArticleResult> Handle(
        PublicBookmarkArticleCommand command,
        CancellationToken cancellationToken
    )
    {
        await articleInteractionRepository.ExistsOrThrowAsync(
            articleId: command.ArticleId,
            cancellationToken: cancellationToken
        );

        bool alreadyBookmarked = await articleInteractionRepository.HasBookmarkedAsync(
            userId: command.UserId,
            articleId: command.ArticleId,
            cancellationToken: cancellationToken
        );

        if (alreadyBookmarked)
        {
            throw i18n.ArticleInteraction.AlreadyBookmarked();
        }

        var bookmark = ArticleBookmarkEntity.Create(
            id: Guid.NewGuid(),
            userId: command.UserId,
            articleId: command.ArticleId
        );

        await articleInteractionRepository.AddBookmarkAsync(bookmark: bookmark, cancellationToken: cancellationToken);

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new PublicBookmarkArticleResult(IsSuccess: true);
    }
}
