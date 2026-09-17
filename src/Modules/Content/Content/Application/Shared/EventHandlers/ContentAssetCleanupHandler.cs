using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Events;
using _116.Core.Contracts.Application.Services;
using _116.Core.Contracts.Domain.Enums;
using _116.Shared.Application.Services;

namespace _116.Content.Application.Shared.EventHandlers;

/// <summary>
/// Cleans up the remote assets left behind by committed content mutations.
/// Runs post-commit in its own scope, so the business change is already
/// durable when cleanup starts and a storage failure can only orphan a
/// remote asset — a cost problem — never leave live content with dead asset
/// URLs. Database work always precedes remote calls: file rows are
/// soft-deleted (their remote assets ride the file lifecycle events) and
/// orphaned <c>article_images</c> rows are removed and committed before the
/// batch storage delete, so a storage outage cannot leave dangling rows.
/// Failures are logged and tolerated by the publisher; the assets stay
/// re-cleanable.
/// </summary>
/// <param name="fileStorage">Core's storage contract.</param>
/// <param name="articleRepository">Repository for article image rows.</param>
/// <param name="unitOfWork">Unit of Work committing the soft deletions and the orphaned-row removal.</param>
public class ContentAssetCleanupHandler(
    IFileStorageService fileStorage,
    IArticleRepository articleRepository,
    IContentUnitOfWork unitOfWork
)
    : IDomainEventHandler<ArticleDeletedEvent>,
        IDomainEventHandler<VideoDeletedEvent>,
        IDomainEventHandler<ShortVideoDeletedEvent>,
        IDomainEventHandler<ArticleBodyImagesOrphanedEvent>
{
    /// <inheritdoc />
    public async Task Handle(ArticleDeletedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        if (domainEvent.CoverFileId.HasValue)
        {
            await SoftDeleteFilesAsync([domainEvent.CoverFileId.Value], cancellationToken);
        }

        if (domainEvent.BodyImageStorageKeys.Count > 0)
        {
            await fileStorage.DeleteAssetsAsync(
                storageKeys: domainEvent.BodyImageStorageKeys,
                kind: EnumStoredFileKind.Image,
                cancellationToken: cancellationToken
            );
        }
    }

    /// <inheritdoc />
    public async Task Handle(VideoDeletedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        if (domainEvent.ThumbnailFileId.HasValue)
        {
            await SoftDeleteFilesAsync([domainEvent.ThumbnailFileId.Value], cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task Handle(ShortVideoDeletedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        Guid[] fileIds = [.. new[] { domainEvent.VideoFileId, domainEvent.ThumbnailFileId }.OfType<Guid>()];

        if (fileIds.Length > 0)
        {
            await SoftDeleteFilesAsync(fileIds, cancellationToken);
        }
    }

    /// <summary>
    /// Soft-deletes the files in one transaction, so a short video's clip and thumbnail cannot
    /// half-disappear.
    /// </summary>
    /// <param name="fileIds">The files to soft-delete.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    private Task SoftDeleteFilesAsync(IReadOnlyCollection<Guid> fileIds, CancellationToken cancellationToken)
    {
        return unitOfWork.ExecuteInTransactionAsync(
            async transactionToken =>
            {
                foreach (Guid fileId in fileIds)
                {
                    await fileStorage.DeleteAsync(fileId: fileId, cancellationToken: transactionToken);
                }
            },
            cancellationToken: cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task Handle(ArticleBodyImagesOrphanedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<ArticleImageEntity> images = await articleRepository.GetImagesByArticleIdAsync(
            articleId: domainEvent.ArticleId,
            cancellationToken: cancellationToken
        );

        HashSet<string> orphanedKeys = domainEvent.StorageKeys.ToHashSet(StringComparer.OrdinalIgnoreCase);

        List<ArticleImageEntity> orphanedRows = images.Where(image => orphanedKeys.Contains(image.StorageKey)).ToList();

        if (orphanedRows.Count > 0)
        {
            articleRepository.RemoveImages(images: orphanedRows);
            await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
        }

        if (domainEvent.StorageKeys.Count > 0)
        {
            await fileStorage.DeleteAssetsAsync(
                storageKeys: domainEvent.StorageKeys,
                kind: EnumStoredFileKind.Image,
                cancellationToken: cancellationToken
            );
        }
    }
}
