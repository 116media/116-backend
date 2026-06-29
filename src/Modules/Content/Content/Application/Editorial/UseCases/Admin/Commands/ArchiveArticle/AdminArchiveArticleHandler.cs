using _116.Content.Application.Shared.Cache;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ArchiveArticle;

/// <summary>
/// Handles the <see cref="AdminArchiveArticleCommand" /> to archive an article.
/// Note: Archiving is reversible — Cloudinary images are not deleted.
/// </summary>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="cacheInvalidator">Invalidates the popular-articles cache when the article leaves the published set.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminArchiveArticleHandler(
    IArticleRepository articleRepository,
    IContentUnitOfWork unitOfWork,
    IPopularArticlesCacheInvalidator cacheInvalidator,
    ContentI18n i18n
) : ICommandHandler<AdminArchiveArticleCommand, AdminArchiveArticleResult>
{
    /// <inheritdoc />
    public async Task<AdminArchiveArticleResult> Handle(
        AdminArchiveArticleCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid id = Guid.Parse(command.Id);

        ArticleEntity article = await articleRepository.GetByIdOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );

        bool archived = article.Archive();

        if (!archived)
        {
            throw i18n.Article.AlreadyArchived();
        }

        articleRepository.Update(article: article);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        cacheInvalidator.Invalidate();

        return new AdminArchiveArticleResult(IsSuccess: true);
    }
}
