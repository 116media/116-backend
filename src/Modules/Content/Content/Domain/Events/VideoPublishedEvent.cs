using _116.Shared.Domain;

namespace _116.Content.Domain.Events;

/// <summary>
/// Raised when a video enters the published set. Consumed by the
/// popular-videos cache invalidation so the ranked list reflects the new
/// membership on the next read.
/// </summary>
/// <param name="VideoId">The video that was published.</param>
public record VideoPublishedEvent(Guid VideoId) : IDomainEvent;
