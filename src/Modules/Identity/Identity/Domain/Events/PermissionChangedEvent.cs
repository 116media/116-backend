using _116.Shared.Domain;

namespace _116.Identity.Domain.Events;

/// <summary>
/// Raised whenever a permission is created, updated, activated, deactivated, deleted or restored.
/// Consumed by the lookup cache invalidation; the consumer is idempotent, so repeated raises
/// cost only a cache miss.
/// </summary>
/// <param name="PermissionId">The permission that changed.</param>
public record PermissionChangedEvent(Guid PermissionId) : DomainEvent;
