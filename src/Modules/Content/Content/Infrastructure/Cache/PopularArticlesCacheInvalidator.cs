using _116.Content.Application.Shared.Cache;

namespace _116.Content.Infrastructure.Cache;

/// <summary>
/// Singleton popular-articles cache invalidator. Behavior is inherited from
/// <see cref="CacheInvalidator" />; the distinct type gives it an eviction token independent of
/// the videos and tags caches. Registered against
/// <see cref="IPopularArticlesCacheInvalidator" /> and consumed by the popular-articles query
/// handler and the like/comment/share/bookmark/publish/archive/delete article handlers.
/// </summary>
public sealed class PopularArticlesCacheInvalidator : CacheInvalidator, IPopularArticlesCacheInvalidator { }
