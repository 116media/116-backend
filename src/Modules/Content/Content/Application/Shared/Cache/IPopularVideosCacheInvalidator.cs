namespace _116.Content.Application.Shared.Cache;

/// <summary>
/// Invalidates the popular-videos cache when video engagement counters or publish state
/// change.
/// </summary>
/// <remarks>
/// The popular-videos query ranks published videos by a weighted engagement score and caches
/// the result in-process. Any operation that changes an engagement counter (rate, share) or a
/// video's membership in the published set (publish, archive) must call
/// <see cref="ICacheInvalidator.Invalidate" /> after committing, so the next read reflects the
/// updated ranking. A distinct interface (and therefore a distinct DI singleton) keeps this
/// cache's eviction independent of the articles and tags caches.
/// </remarks>
public interface IPopularVideosCacheInvalidator : ICacheInvalidator { }
