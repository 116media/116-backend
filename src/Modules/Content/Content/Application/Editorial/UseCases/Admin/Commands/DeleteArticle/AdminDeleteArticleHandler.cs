using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.DeleteArticle;

/// <summary>
/// Handles the <see cref="AdminDeleteArticleCommand" /> to permanently delete a draft or rejected article.
/// Captures the cover file id and the body image storage keys on the aggregate before removal so the
/// post-commit cleanup handler can soft-delete the cover row and purge the remote assets after
/// the business commit; a storage failure can no longer block or outlive the deletion.
/// Cover-type <c>article_images</c> rows are excluded from the captured keys: their storage key is
/// the cover <c>FileEntity</c> key, whose remote asset is purged once by the file soft-delete
/// reaction.
/// </summary>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminDeleteArticleHandler(
    IArticleRepository articleRepository,
    IContentUnitOfWork unitOfWork,
    ContentI18n i18n
) : ICommandHandler<AdminDeleteArticleCommand, AdminDeleteArticleResult>
{
    /// <inheritdoc />
    public async Task<AdminDeleteArticleResult> Handle(
        AdminDeleteArticleCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid id = Guid.Parse(command.Id);

        ArticleEntity article = await articleRepository.GetByIdOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );

        if (article.Status != EnumContentStatus.Draft && article.Status != EnumContentStatus.Rejected)
        {
            throw i18n.Article.CannotDeletePublishedArticle();
        }

        IReadOnlyList<ArticleImageEntity> images = await articleRepository.GetImagesByArticleIdAsync(
            articleId: article.Id,
            cancellationToken: cancellationToken
        );

        List<string> bodyImageStorageKeys = images
            .Where(img => img.ImageType == EnumArticleImageType.Body)
            .Select(img => img.StorageKey)
            .ToList();

        article.MarkDeleted(bodyImageStorageKeys: bodyImageStorageKeys);
        articleRepository.Remove(article: article);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminDeleteArticleResult(IsSuccess: true);
    }
}
