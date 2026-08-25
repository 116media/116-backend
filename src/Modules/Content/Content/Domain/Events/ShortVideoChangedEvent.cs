using _116.Shared.Domain;

namespace _116.Content.Domain.Events;

/// <summary>
/// Raised whenever a short video's metadata, media files or feed visibility change.
/// Consumed by the shorts cache invalidation; the consumer is idempotent, so repeated
/// raises cost only a cache miss.
/// </summary>
/// <param name="ShortVideoId">The short video that changed.</param>
public record ShortVideoChangedEvent(Guid ShortVideoId) : IDomainEvent;
