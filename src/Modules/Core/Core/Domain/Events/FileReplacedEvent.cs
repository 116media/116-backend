using _116.Shared.Domain;

namespace _116.Core.Domain.Events;

/// <summary>
/// Raised when a file row is superseded by a newer upload. The old storage
/// key is captured at raise time so the post-commit cleanup consumer can
/// delete the old remote asset. A <c>null</c> key means the row referenced an
/// external URL (for example a social-provider avatar) with nothing to delete
/// remotely, and consumers skip it.
/// </summary>
/// <param name="FileId">The replaced file row.</param>
/// <param name="OldStorageKey">The storage key of the replaced remote asset, or <c>null</c> for external-URL rows.</param>
public record FileReplacedEvent(Guid FileId, string? OldStorageKey) : IDomainEvent;
