using _116.Shared.Domain;

namespace _116.Content.Domain.Events;

/// <summary>
/// Raised when a video leaves the published set (rejection or archival of a
/// published video). Consumed by the popular-videos cache invalidation so
/// the departed video stops appearing in the ranked list.
/// </summary>
/// <param name="VideoId">The video that left the published set.</param>
public record VideoUnpublishedEvent(Guid VideoId) : IDomainEvent;
