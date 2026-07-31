using _116.Shared.Domain;

namespace _116.Content.Domain.Events;

/// <summary>
/// Raised whenever the tag graph changes: a tag is created, updated or
/// deleted, or a tag association is added to or removed from an article,
/// video or lyrics page. Consumed by the tags cache invalidation (the
/// popular-tags and all-tags projections share one eviction token). A bulk
/// tag replacement raises one event per changed association; the consumer is
/// idempotent so the extra raises cost nothing beyond a cache miss.
/// </summary>
/// <param name="TagId">The tag whose graph membership changed.</param>
public record TagGraphChangedEvent(Guid TagId) : IDomainEvent;
