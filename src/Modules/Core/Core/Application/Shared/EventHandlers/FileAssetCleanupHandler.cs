using _116.Core.Application.Shared.Services;
using _116.Core.Domain.Events;
using _116.Shared.Application.Services;

namespace _116.Core.Application.Shared.EventHandlers;

/// <summary>
/// Deletes the remote asset behind a replaced or soft-deleted file row.
/// Runs post-commit in its own scope, so the row change is already durable
/// and a storage failure can only orphan a remote asset, never corrupt the
/// row state. Rows without a storage key reference an external URL (for
/// example social-provider avatars) and are skipped — there is nothing to
/// delete remotely. Failures are logged and tolerated by the publisher.
/// </summary>
/// <param name="fileService">Service deleting files from cloud storage by storage key.</param>
public class FileAssetCleanupHandler(IFileService fileService)
    : IDomainEventHandler<FileReplacedEvent>,
        IDomainEventHandler<FileSoftDeletedEvent>
{
    /// <inheritdoc />
    public async Task Handle(FileReplacedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        if (domainEvent.OldStorageKey is null)
        {
            return;
        }

        await fileService.DeleteFileAsync(storageKey: domainEvent.OldStorageKey, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task Handle(FileSoftDeletedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        if (domainEvent.StorageKey is null)
        {
            return;
        }

        await fileService.DeleteFileAsync(storageKey: domainEvent.StorageKey, cancellationToken: cancellationToken);
    }
}
