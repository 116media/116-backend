using _116.Shared.Domain;

namespace _116.Identity.Domain.Events;

/// <summary>
/// Raised whenever a role is created, updated, activated, deactivated, deleted, restored or has its permission set changed.
/// Consumed by the lookup cache invalidation; the consumer is idempotent, so repeated raises
/// cost only a cache miss.
/// </summary>
/// <param name="RoleId">The role that changed.</param>
public record RoleChangedEvent(Guid RoleId) : IDomainEvent;
