using _116.Identity.Application.Shared.Cache;
using _116.Identity.Domain.Events;
using _116.Shared.Application.Services;
using Microsoft.Extensions.Caching.Hybrid;

namespace _116.Identity.Application.Shared.EventHandlers;

/// <summary>
/// Evicts the cached role and permission lookups whenever either aggregate changes.
/// Eviction is idempotent, so handling the same fact more than once costs only a cache miss.
/// </summary>
/// <param name="cache">The hybrid cache holding the identity lookup projections.</param>
public class IdentityLookupCacheHandler(HybridCache cache)
    : IDomainEventHandler<RoleChangedEvent>,
        IDomainEventHandler<PermissionChangedEvent>
{
    /// <inheritdoc />
    public async Task Handle(RoleChangedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await cache.RemoveByTagAsync(IdentityCacheTags.Lookups, cancellationToken);
    }

    /// <inheritdoc />
    public async Task Handle(PermissionChangedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await cache.RemoveByTagAsync(IdentityCacheTags.Lookups, cancellationToken);
    }
}
