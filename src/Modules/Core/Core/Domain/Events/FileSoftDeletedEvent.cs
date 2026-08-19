using _116.Shared.Domain;

namespace _116.Core.Domain.Events;

/// <summary>
/// Raised when a file row is soft-deleted outside a replacement flow. The
/// storage key is captured at raise time so the post-commit cleanup consumer
/// can delete the remote asset. A <c>null</c> key means the row referenced an
/// external URL with nothing to delete remotely, and consumers skip it.
/// </summary>
/// <param name="FileId">The soft-deleted file row.</param>
/// <param name="StorageKey">The storage key of the remote asset, or <c>null</c> for external-URL rows.</param>
public record FileSoftDeletedEvent(Guid FileId, string? StorageKey) : IDomainEvent;
