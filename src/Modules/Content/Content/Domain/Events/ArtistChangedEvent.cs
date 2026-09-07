using _116.Shared.Domain;

namespace _116.Content.Domain.Events;

/// <summary>
/// Raised whenever an artist's profile or discography changes. Consumed by the artist cache
/// invalidation; the consumer is idempotent, so repeated raises cost only a cache miss.
/// </summary>
/// <param name="ArtistId">The artist whose projection changed.</param>
public record ArtistChangedEvent(Guid ArtistId) : IDomainEvent;
