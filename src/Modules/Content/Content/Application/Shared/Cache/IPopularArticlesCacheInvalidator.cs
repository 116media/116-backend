namespace _116.Content.Application.Shared.Cache;

/// <summary>
/// Provides a mechanism to invalidate the popular-articles cache when article engagement
/// counters or publish state change.
/// </summary>
/// <remarks>
/// The popular-articles query ranks published articles by a weighted engagement score and
/// caches the result in-process. Any operation that changes an engagement counter
/// (like, comment, share, bookmark) or an article's membership in the published set
/// (publish, archive, delete) must call <see cref="Invalidate" /> after committing, so the
/// next read reflects the updated ranking. The implementation uses a
/// <see cref="CancellationToken" /> registered as a
/// <see cref="Microsoft.Extensions.Caching.Memory.MemoryCacheEntryOptions" /> expiration
/// token, so a single cancellation evicts every entry regardless of the limit, category,
/// or exclude-id used when the entry was stored.
/// </remarks>
public interface IPopularArticlesCacheInvalidator
{
    /// <summary>
    /// Returns a <see cref="CancellationToken" /> that cache entries should register as an
    /// expiration token. When <see cref="Invalidate" /> is called the token is cancelled,
    /// evicting all associated entries immediately.
    /// </summary>
    /// <returns>The current eviction <see cref="CancellationToken" />.</returns>
    CancellationToken GetEvictionToken();

    /// <summary>
    /// Cancels the current eviction token, immediately evicting every cached
    /// popular-articles entry, and prepares a fresh token for the next cache fill.
    /// </summary>
    void Invalidate();
}
