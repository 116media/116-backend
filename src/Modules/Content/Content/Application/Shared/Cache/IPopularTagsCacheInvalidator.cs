namespace _116.Content.Application.Shared.Cache;

/// <summary>
/// Invalidates the popular-tags cache when the tag graph changes.
/// </summary>
/// <remarks>
/// The popular-tags query runs an expensive GROUP BY aggregation and caches results
/// in-process. Any operation that modifies tag–article or tag–video associations must call
/// <see cref="ICacheInvalidator.Invalidate" /> after committing so that the next read reflects
/// the updated usage counts. A distinct interface (and therefore a distinct DI singleton) keeps
/// this cache's eviction independent of the articles and videos caches.
/// </remarks>
public interface IPopularTagsCacheInvalidator : ICacheInvalidator { }
