# Target design

Two patterns, both already established — one in this codebase, one in the framework.

| | Pattern | Source |
| --- | --- | --- |
| Composition | **Decorator** (GoF) over `IRequestHandler<,>` | already used 3× in [CqrsExtension.cs:62-64](../../src/Shared/Shared/Application/Extensions/CqrsExtension.cs#L62-L64) |
| Cache | **`HybridCache`** — L1 memory + L2 Redis, stampede protection, tag invalidation | `Microsoft.Extensions.Caching.Hybrid`, first-party |
| Opt-in | **Marker interface** on the query record | standard idiom |

No cache abstraction is written by hand. There is no reader layer, no cache-region type, no
invalidator hierarchy and no memory-vs-distributed tiering — `HybridCache` is both tiers behind one
API, and a tag replaces every eviction token.

---

## What `HybridCache` provides

Verified against `Microsoft.Extensions.Caching.Abstractions` 9.0.19:

```csharp
ValueTask<T> GetOrCreateAsync<T>(
    string key,
    Func<CancellationToken, ValueTask<T>> factory,
    HybridCacheEntryOptions? options = null,
    IEnumerable<string>? tags = null,
    CancellationToken cancellationToken = default);

ValueTask<T> GetOrCreateAsync<TState, T>(
    string key,
    TState state,
    Func<TState, CancellationToken, ValueTask<T>> factory,
    HybridCacheEntryOptions? options = null,
    IEnumerable<string>? tags = null,
    CancellationToken cancellationToken = default);

ValueTask RemoveByTagAsync(string tag, CancellationToken cancellationToken = default);
ValueTask RemoveAsync(string key, CancellationToken cancellationToken = default);
```

`HybridCacheEntryOptions` carries `Expiration` (L2) and `LocalCacheExpiration` (L1) separately.

Three things it does that the hand-rolled code does not:

- **Stampede protection.** Concurrent misses on one key run the factory once. Today ten simultaneous
  requests for a cold popular-articles feed run the ranking query ten times.
- **Tag invalidation across instances.** `RemoveByTagAsync("…")` reaches L1 and L2. A
  `CancellationTokenSource` cannot leave its process — that is the entire multi-instance problem.
- **L1 + L2 behind one call.** No per-read decision about which store to consult.

L2 is whatever `IDistributedCache` is registered, and
`Microsoft.Extensions.Caching.StackExchangeRedis` is already a `PackageReference` in
[Shared.csproj:31](../../src/Shared/Shared/Shared.csproj#L31). With no `IDistributedCache`
registered, `HybridCache` runs L1-only — so this ships before Redis and gains L2 later by adding one
registration.

---

## Step 1 — the marker interface

The query record declares what it needs cached. Nothing else does.

```csharp
// src/Shared/Shared.Contracts/Application/CQRS/ICacheableRequest.cs
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
```

Tag names are shared constants, so the query that stores and the handler that evicts cannot drift:

```csharp
// src/Modules/Content/Content/Application/Shared/Cache/ContentCacheTags.cs
namespace _116.Content.Application.Shared.Cache;

/// <summary>
/// Tag names shared by the queries that cache a result and the event handlers that evict it.
/// </summary>
public static class ContentCacheTags
{
    /// <summary>
    /// Feeds ranked by article engagement or article publish state.
    /// </summary>
    public const string PopularArticles = "content:popular-articles";

    /// <summary>
    /// Feeds ranked by video engagement or video publish state.
    /// </summary>
    public const string PopularVideos = "content:popular-videos";

    /// <summary>
    /// Any projection over the tag vocabulary or its content associations.
    /// </summary>
    public const string Tags = "content:tags";

    /// <summary>
    /// The lookup tables: content types, pricing tiers, promotion levels, categories.
    /// </summary>
    public const string Lookups = "content:lookups";
}
```

## Step 2 — the query opts in

```csharp
// …/GetPopularArticles/PublicGetPopularArticlesQuery.cs
/// <summary>
/// Retrieves the most popular published articles ranked by weighted engagement score.
/// </summary>
/// <param name="Limit">Maximum number of articles to return.</param>
/// <param name="CategoryId">Restricts the feed to one category when supplied.</param>
/// <param name="ExcludeId">Article to omit, typically the one being viewed.</param>
public record PublicGetPopularArticlesQuery(int Limit, Guid? CategoryId, Guid? ExcludeId)
    : IQuery<PublicGetPopularArticlesResult>,
        ICacheableRequest
{
    /// <inheritdoc />
    public string CacheKey =>
        $"popular_articles:{Limit}:{CategoryId?.ToString() ?? "all"}:{ExcludeId?.ToString() ?? "none"}";

    /// <inheritdoc />
    public TimeSpan Ttl => TimeSpan.FromMinutes(10);

    /// <inheritdoc />
    public IReadOnlyList<string> CacheTags => [ContentCacheTags.PopularArticles];
}
```

Where cacheability depends on a parameter value — `PublicGetAllTagsQuery` skips the cache when a
search term is present — the guard lives on the query, not in the decorator:

```csharp
public record PublicGetAllTagsQuery(string? Search, int? Limit, EnumCoreContentType? ContentType)
    : IQuery<PublicGetAllTagsResult>,
        IConditionallyCacheableRequest
{
    /// <inheritdoc />
    /// <remarks>
    /// Free-text search produces an unbounded key space, so those results are never stored.
    /// </remarks>
    public bool IsCacheable => string.IsNullOrWhiteSpace(Search);

    /// <inheritdoc />
    public string CacheKey =>
        $"all_tags:{Limit?.ToString() ?? "all"}:{ContentType?.ToString()?.ToLowerInvariant() ?? "any"}";

    /// <inheritdoc />
    public TimeSpan Ttl => TimeSpan.FromMinutes(10);

    /// <inheritdoc />
    public IReadOnlyList<string> CacheTags => [ContentCacheTags.Tags];
}
```

## Step 3 — the decorator

Fourth in the existing chain, and the only caching code in the solution.

```csharp
// src/Shared/Shared/Application/Decorators/CachingDecorator.cs
using _116.Shared.Contracts.Application.CQRS;
using Microsoft.Extensions.Caching.Hybrid;

namespace _116.Shared.Application.Decorators;

/// <summary>
/// Decorator that serves <see cref="ICacheableRequest" /> results from the hybrid cache and
/// stores every miss. Requests that do not opt in pass straight through.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
/// <param name="handler">The decorated handler.</param>
/// <param name="cache">The hybrid cache backing every cached result.</param>
public class CachingDecorator<TRequest, TResponse>(
    IRequestHandler<TRequest, TResponse> handler,
    HybridCache cache
) : IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : notnull
{
    /// <summary>
    /// Handles the request, returning the stored result when one exists for its key.
    /// </summary>
    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken = default)
    {
        if (request is not ICacheableRequest cacheable || request is IConditionallyCacheableRequest { IsCacheable: false })
        {
            return await handler.Handle(request, cancellationToken);
        }

        var options = new HybridCacheEntryOptions
        {
            Expiration = cacheable.Ttl,
            LocalCacheExpiration = cacheable.Ttl,
        };

        // The state overload keeps the factory static, so no closure is allocated per request.
        return await cache.GetOrCreateAsync(
            key: cacheable.CacheKey,
            state: (handler, request),
            factory: static async (state, token) => await state.handler.Handle(state.request, token),
            options: options,
            tags: cacheable.CacheTags,
            cancellationToken: cancellationToken
        );
    }
}
```

```csharp
// src/Shared/Shared/Application/Extensions/CqrsExtension.cs
services.Decorate(typeof(IRequestHandler<,>), typeof(AccountRateLimitDecorator<,>));
services.Decorate(typeof(IRequestHandler<,>), typeof(ValidationDecorator<,>));
services.Decorate(typeof(IRequestHandler<,>), typeof(LoggingDecorator<,>));
services.Decorate(typeof(IRequestHandler<,>), typeof(CachingDecorator<,>));
```

> **Position is a decision, not line order.** Registered last, caching is outermost: a hit skips
> validation and logging entirely. That is acceptable only because the key derives from an
> already-shaped query, and it means cache hits do not appear in the request log. Registering it
> before `LoggingDecorator` instead keeps hits observable at the cost of running the log path on
> every hit. Pick one and write down why.

## Step 4 — registration

```csharp
// src/Api/Program.cs
builder.Services.AddHybridCache(options =>
{
    options.DefaultEntryOptions = new HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromMinutes(10),
        LocalCacheExpiration = TimeSpan.FromMinutes(10),
    };
});
```

`AddMemoryCache()` stays — `HybridCache` uses it as L1. When Redis lands, adding
`AddStackExchangeRedisCache(...)` gives L2 with no change to any code above.

## Step 5 — the handlers lose their caching

```csharp
/// <summary>
/// Handles the <see cref="PublicGetPopularArticlesQuery" /> to retrieve the most popular
/// published articles ranked by a weighted engagement score.
/// </summary>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="fileRepository">Repository for resolving cover image URLs.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class PublicGetPopularArticlesHandler(
    IArticleRepository articleRepository,
    IFileRepository fileRepository,
    IMapper mapper
) : IQueryHandler<PublicGetPopularArticlesQuery, PublicGetPopularArticlesResult>
{
    /// <inheritdoc />
    public async Task<PublicGetPopularArticlesResult> Handle(
        PublicGetPopularArticlesQuery query,
        CancellationToken cancellationToken
    )
    {
        IReadOnlyList<ArticleEntity> articles = await articleRepository.GetPopularArticlesAsync(
            limit: query.Limit,
            excludeId: query.ExcludeId,
            categoryId: query.CategoryId,
            cancellationToken: cancellationToken
        );

        return new PublicGetPopularArticlesResult(
            Articles: await articles.ToArticleSummaryDtosAsync(mapper, fileRepository, cancellationToken)
        );
    }
}
```

Five constructor dependencies down to three; the TTL constant, the key construction and the
get/miss/set block are gone.

## Step 6 — invalidation

The three existing event handlers keep their job and change one line each.

```csharp
/// <summary>
/// Evicts the cached popular-articles feeds when an article's membership in the published set
/// changes. Eviction is idempotent, so handling the same fact twice costs only a cache miss.
/// </summary>
/// <param name="cache">The hybrid cache holding the feeds.</param>
public class PopularArticlesCacheHandler(HybridCache cache)
    : IDomainEventHandler<ArticlePublishedEvent>,
        IDomainEventHandler<ArticleUnpublishedEvent>,
        IDomainEventHandler<ArticleDeletedEvent>
{
    /// <inheritdoc />
    public async Task Handle(ArticlePublishedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await cache.RemoveByTagAsync(ContentCacheTags.PopularArticles, cancellationToken);
    }

    // The remaining Handle overloads are identical.
}
```

These fire from `DispatchDomainEventsInterceptor.SavedChangesAsync` — after the commit succeeds,
discarded when it fails. That timing is already correct and does not change.

---

## The lookup tables

Nothing new is required. `ContentTypes`, `PricingTiers`, `PromotionLevels` and categories are read
constantly and mutated a few times a year, and none of them is cached today. Each read query
implements `ICacheableRequest` with `ContentCacheTags.Lookups` and a 30-minute TTL; each admin
mutation handler calls `RemoveByTagAsync(ContentCacheTags.Lookups, …)`.

Because the decorator caches the **query result** — `AdminGetAllContentTypesResult`, a flat record
of DTOs — no aggregate is ever serialized. That removes the entire problem that made
`CachedBasketRepository` need two hand-written `JsonConverter`s and reflection into a private field,
and it is the reason this design does not need the two-tier split an entity cache would have forced.

**Deliberately not cached:**

| Read | Why not |
| --- | --- |
| Anything carrying `IsLiked` / `IsBookmarked` | Per-user; the key space is users × articles and the value is wrong for everyone else |
| Free-text search on any list | Unbounded key space — `IsCacheable` returns false |
| Admin paginated lists | Low hit rate, high staleness cost, and the reader is usually the person who just wrote |
| Article and video detail pages | Engagement counters move continuously; the cache would thrash |

---

## What this deletes

| Deleted | Replaced by |
| --- | --- |
| `ICacheInvalidator`, `CacheInvalidator`, the three `IPopular*CacheInvalidator` markers and their subclasses | a tag constant |
| `CancellationChangeToken` eviction, the `Lock`, the `CancellationTokenSource` swap | `RemoveByTagAsync` |
| `IMemoryCache` in four handlers | `HybridCache`, behind the decorator |
| the get/miss/set block, four times | `GetOrCreateAsync` |

Seven types and roughly 120 lines removed. Added: one decorator, two marker interfaces, one
constants class.

---

## Serialization requirement

`HybridCache` serializes cached values (System.Text.Json by default) so an entry can reach L2. Every
cached value here is a query result record — `PublicGetPopularArticlesResult`,
`AdminGetAllContentTypesResult` — flat records of `Guid`, `string`, `int`, `bool`,
`DateTimeOffset?` and enums, already serialized on the wire by the endpoint. No converter, no
attribute, no reflection.

**This is the constraint that keeps the design honest:** a result type that cannot round-trip
through JSON must not be cached. That rules out caching anything holding an aggregate, which is
exactly the rule the reference projects had to enforce by hand.

---

## SOLID

| Principle | How |
| --- | --- |
| **SRP** | The handler answers the query, the decorator caches, the event handler decides when data went stale. Changing a TTL edits the query record and nothing else. |
| **OCP** | No handler is modified to gain caching — a query opts in by implementing an interface, and the decorator is registered once. |
| **LSP** | The decorator is an `IRequestHandler<,>` returning what the inner handler would have returned, at most `Ttl` old. A query that cannot tolerate staleness does not implement the marker, so the substitution never arises. |
| **ISP** | `ICacheableRequest` is three members, implemented only by queries that cache; `IConditionallyCacheableRequest` adds the single member conditional queries need. Nothing implements a member it does not use. |
| **DIP** | Handlers depend on nothing cache-related. `HybridCache` is a framework abstraction with a swappable L2; after this change no application-layer file references a cache implementation. |
