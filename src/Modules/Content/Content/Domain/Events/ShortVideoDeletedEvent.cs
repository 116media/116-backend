using _116.Shared.Domain;

namespace _116.Content.Domain.Events;

/// <summary>
/// Raised when a short video record is removed. The video and thumbnail file
/// ids are captured before removal so post-commit consumers can clean the
/// remote assets without re-querying rows that no longer exist. Consumed by
/// the remote-asset cleanup handler.
/// </summary>
/// <param name="ShortVideoId">The short video that was removed.</param>
/// <param name="VideoFileId">The video file record, or <c>null</c> when no video file was uploaded.</param>
/// <param name="ThumbnailFileId">The thumbnail file record, or <c>null</c> when the short video had no thumbnail.</param>
public record ShortVideoDeletedEvent(Guid ShortVideoId, Guid? VideoFileId, Guid? ThumbnailFileId) : IDomainEvent;
