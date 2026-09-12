using _116.Core.Application.Shared.Cache;
using _116.Core.Domain.Events;
using _116.Shared.Application.Services;
using Microsoft.Extensions.Caching.Hybrid;

namespace _116.Core.Application.Shared.EventHandlers;

/// <summary>
/// Evicts cached file projections whenever a file is soft-deleted or superseded, so no consumer
/// keeps serving the URL of an asset that is gone. Eviction is idempotent, so handling the same
/// fact more than once costs only a cache miss.
/// </summary>
/// <param name="cache">The hybrid cache holding the file projections.</param>
public class FileCacheHandler(HybridCache cache)
    : IDomainEventHandler<FileSoftDeletedEvent>,
        IDomainEventHandler<FileReplacedEvent>
{
    /// <inheritdoc />
    public async Task Handle(FileSoftDeletedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await cache.RemoveByTagAsync(CoreCacheTags.Files, cancellationToken);
    }

    /// <inheritdoc />
    public async Task Handle(FileReplacedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await cache.RemoveByTagAsync(CoreCacheTags.Files, cancellationToken);
    }
}
