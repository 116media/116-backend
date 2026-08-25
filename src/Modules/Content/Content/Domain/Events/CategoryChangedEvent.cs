using _116.Shared.Domain;

namespace _116.Content.Domain.Events;

/// <summary>
/// Raised whenever a category or its pricing changes in any way that alters a lookup projection. Consumed by the lookup cache
/// invalidation; the consumer is idempotent, so repeated raises cost only a cache miss.
/// </summary>
/// <param name="CategoryId">The category that changed.</param>
public record CategoryChangedEvent(Guid CategoryId) : IDomainEvent;
