using _116.Content.Application.Shared.Cache;

namespace _116.Content.Infrastructure.Cache;

/// <summary>
/// Singleton popular-videos cache invalidator. Behavior is inherited from
/// <see cref="CacheInvalidator" />; the distinct type gives it an eviction token independent of
/// the articles and tags caches. Registered against <see cref="IPopularVideosCacheInvalidator" />
/// and consumed by the popular-videos query handler and the rate/share/publish/archive video
/// handlers.
/// </summary>
public sealed class PopularVideosCacheInvalidator : CacheInvalidator, IPopularVideosCacheInvalidator { }
