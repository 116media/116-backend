using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Core.Application.Shared.Services;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.DeleteArticle;

/// <summary>
/// Handles the <see cref="AdminDeleteArticleCommand" /> to permanently delete a draft or rejected article.
/// Cleans up all associated Cloudinary image assets before removing the database record.
/// </summary>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="cloudinaryService">Service for deleting Cloudinary image assets.</param>
public class AdminDeleteArticleHandler(
    IArticleRepository articleRepository,
    IContentUnitOfWork unitOfWork,
    ICloudinaryService cloudinaryService
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
            throw ArticleErrors.CannotDeletePublishedArticle();
        }

        IReadOnlyList<ArticleImageEntity> images = await articleRepository.GetImagesByArticleIdAsync(
            articleId: article.Id,
            cancellationToken: cancellationToken
        );

        List<string> storageKeys = images.Select(img => img.StorageKey).ToList();

        if (storageKeys.Count > 0)
        {
            await cloudinaryService.DeleteImagesAsync(publicIds: storageKeys, cancellationToken: cancellationToken);
        }

        articleRepository.Remove(article: article);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminDeleteArticleResult(IsSuccess: true);
    }
}
