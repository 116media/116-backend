namespace _116.Content.Application.Shared.Cache;

/// <summary>
/// Invalidates the popular-articles cache when article engagement counters or publish state
/// change.
/// </summary>
/// <remarks>
/// The popular-articles query ranks published articles by a weighted engagement score and
/// caches the result in-process. Any operation that changes an engagement counter (like,
/// comment, share, bookmark) or an article's membership in the published set (publish, archive,
/// delete) must call <see cref="ICacheInvalidator.Invalidate" /> after committing, so the next
/// read reflects the updated ranking. A distinct interface (and therefore a distinct DI
/// singleton) keeps this cache's eviction independent of the videos and tags caches.
/// </remarks>
public interface IPopularArticlesCacheInvalidator : ICacheInvalidator { }
