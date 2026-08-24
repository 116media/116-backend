namespace _116.Shared.Contracts.Application.CQRS;

/// <summary>
/// A request whose result may be served from cache. The request owns its key, lifetime and
/// invalidation tags because it owns the parameters they are derived from.
/// </summary>
public interface ICacheableRequest
{
    /// <summary>
    /// The cache key, unique across every distinct result this request can produce.
    /// </summary>
    string CacheKey { get; }

    /// <summary>
    /// How long the result stays valid absent an explicit invalidation.
    /// </summary>
    TimeSpan Ttl { get; }

    /// <summary>
    /// Tags this result is evicted by when the underlying data changes.
    /// </summary>
    IReadOnlyList<string> CacheTags { get; }
}

/// <summary>
/// A cacheable request that declines the cache for some parameter values.
/// </summary>
public interface IConditionallyCacheableRequest : ICacheableRequest
{
    /// <summary>
    /// Whether this instance's result may be stored.
    /// </summary>
    bool IsCacheable { get; }
}
