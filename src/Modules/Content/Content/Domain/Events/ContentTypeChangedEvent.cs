using _116.Shared.Domain;

namespace _116.Content.Domain.Events;

/// <summary>
/// Raised whenever a content type is created, renamed, activated or deactivated. Consumed by the lookup cache
/// invalidation; the consumer is idempotent, so repeated raises cost only a cache miss.
/// </summary>
/// <param name="ContentTypeId">The content type that changed.</param>
public record ContentTypeChangedEvent(Guid ContentTypeId) : IDomainEvent;
