namespace _116.Content.Application.Shared.Cache;

/// <summary>
/// A cache invalidator that exposes a shared eviction <see cref="CancellationToken" /> for
/// in-process <see cref="Microsoft.Extensions.Caching.Memory.IMemoryCache" /> entries.
/// </summary>
/// <remarks>
/// A query handler that caches an expensive result registers the token returned by
/// <see cref="GetEvictionToken" /> as a
/// <see cref="Microsoft.Extensions.Caching.Memory.MemoryCacheEntryOptions">expiration token</see>.
/// Calling <see cref="Invalidate" /> cancels that token, evicting every entry that registered
/// it at once — regardless of the parameters used when each entry was stored — and prepares a
/// fresh token for the next cache fill. Each domain that needs its own independently-evictable
/// cache declares a distinct marker interface extending this one, so DI resolves a separate
/// singleton (and therefore a separate token) per domain.
/// </remarks>
public interface ICacheInvalidator
{
    /// <summary>
    /// Returns the current eviction <see cref="CancellationToken" /> for cache entries to
    /// register as an expiration token. When <see cref="Invalidate" /> is called the token is
    /// cancelled, evicting all associated entries immediately.
    /// </summary>
    /// <returns>The current eviction <see cref="CancellationToken" />.</returns>
    CancellationToken GetEvictionToken();

    /// <summary>
    /// Cancels the current eviction token — immediately evicting every entry that registered
    /// it — and prepares a fresh token for the next cache fill.
    /// </summary>
    void Invalidate();
}
