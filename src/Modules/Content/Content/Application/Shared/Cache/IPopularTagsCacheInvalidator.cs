namespace _116.Content.Application.Shared.Cache;

/// <summary>
/// Provides a mechanism to invalidate the popular-tags cache when the tag graph changes.
/// </summary>
/// <remarks>
/// The popular-tags query runs an expensive GROUP BY aggregation and caches results
/// in-process. Any operation that modifies tag–article or tag–video associations must
/// call <see cref="Invalidate" /> after committing so that the next read reflects the
/// updated usage counts. The implementation uses a <see cref="CancellationToken" />
/// registered as a <see cref="Microsoft.Extensions.Caching.Memory.MemoryCacheEntryOptions" />
/// expiration token, so a single cancellation evicts every entry regardless of the
/// <c>limit</c> parameter used when the entry was stored.
/// </remarks>
public interface IPopularTagsCacheInvalidator
{
    /// <summary>
    /// Returns a <see cref="CancellationToken" /> that cache entries
    /// should register as an expiration token. When <see cref="Invalidate" /> is called
    /// the token is cancelled, evicting all associated entries immediately.
    /// </summary>
    /// <returns>
    /// The current eviction <see cref="CancellationToken" />.
    /// </returns>
    CancellationToken GetEvictionToken();

    /// <summary>
    /// Cancels the current eviction token, immediately evicting every cached popular-tags
    /// entry, and prepares a fresh token for the next cache fill.
    /// </summary>
    void Invalidate();
}
