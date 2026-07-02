using _116.Content.Application.Shared.Cache;

namespace _116.Content.Infrastructure.Cache;

/// <summary>
/// Singleton popular-tags cache invalidator. Behavior is inherited from
/// <see cref="CacheInvalidator" />; the distinct type gives it an eviction token independent of
/// the articles and videos caches. Registered against <see cref="IPopularTagsCacheInvalidator" />
/// and consumed by the popular-tags query handler and the tag-association mutation handlers.
/// </summary>
public sealed class PopularTagsCacheInvalidator : CacheInvalidator, IPopularTagsCacheInvalidator { }
