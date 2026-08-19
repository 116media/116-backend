using _116.Shared.Domain;

namespace _116.Content.Domain.Events;

/// <summary>
/// Raised when a video record is removed. The thumbnail file id is captured
/// before removal so post-commit consumers can clean the remote asset
/// without re-querying a row that no longer exists. Consumed today by the
/// popular-videos cache invalidation; the remote-asset cleanup consumer
/// attaches to the same event.
/// </summary>
/// <param name="VideoId">The video that was removed.</param>
/// <param name="ThumbnailFileId">The thumbnail file record, or <c>null</c> when the video had no thumbnail.</param>
public record VideoDeletedEvent(Guid VideoId, Guid? ThumbnailFileId) : IDomainEvent;
