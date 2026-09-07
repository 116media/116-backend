# Current state

## Where caching lives today

Four public query handlers, each carrying its own copy of the same read-through block:

| Handler | Cache key |
| --- | --- |
| `PublicGetPopularArticlesHandler` | `popular_articles_{limit}_{category}_{exclude}` |
| `PublicGetPopularVideosHandler` | `popular_videos_{limit}_{category}_{exclude}` |
| `PublicGetPopularTagsHandler` | `popular_tags_{limit}_{contentType}` |
| `PublicGetAllTagsHandler` | `all_tags_{limit}_{contentType}` |

```csharp
// src/Modules/Content/Content/Application/Editorial/UseCases/Public/Queries/
//   GetPopularArticles/PublicGetPopularArticlesHandler.cs:47-73
string categoryPart = query.CategoryId?.ToString() ?? "all";
string excludePart = query.ExcludeId?.ToString() ?? "none";
string cacheKey = $"popular_articles_{query.Limit}_{categoryPart}_{excludePart}";

if (cache.TryGetValue(cacheKey, out IReadOnlyList<ArticleSummaryDto>? cached) && cached is not null)
{
    return new PublicGetPopularArticlesResult(Articles: cached);
}

IReadOnlyList<ArticleEntity> articles = await articleRepository.GetPopularArticlesAsync(…);
IReadOnlyList<ArticleSummaryDto> dtoList = await articles.ToArticleSummaryDtosAsync(mapper, fileRepository, cancellationToken);

var options = new MemoryCacheEntryOptions()
    .SetAbsoluteExpiration(CacheTtl)
    .AddExpirationToken(new CancellationChangeToken(cacheInvalidator.GetEvictionToken()));

cache.Set(cacheKey, dtoList, options);
```

Invalidation, by contrast, is already well factored — and is the one part of this backend that
beats both reference projects:

```text
Application/Shared/Cache/       ICacheInvalidator + 3 region markers
Infrastructure/Cache/           CacheInvalidator (CancellationTokenSource) + 3 subclasses
Application/Shared/EventHandlers/  PopularArticlesCacheHandler, PopularVideosCacheHandler, PopularTagsCacheHandler
```

`DispatchDomainEventsInterceptor` collects events in `SavingChanges` and dispatches them in
`SavedChangesAsync`, so eviction fires **after** the commit succeeds and is discarded when it fails.
Neither reference project has commit-safe invalidation. **Keep this.**

## The problems

### 1. The read-through block is duplicated four times

Four handlers each own a key format, a TTL constant, an `IMemoryCache` dependency and a
get/miss/set sequence. A change to the caching strategy is a four-file edit with four chances to
diverge, and it already has: `PublicGetAllTagsHandler` added a `cacheable` guard that skips
caching for search terms, and the other three have no equivalent concept.

### 2. Handlers are coupled to one caching technology

Not a broken dependency direction — `IMemoryCache` and `MemoryCacheEntryOptions` both live in
`Microsoft.Extensions.Caching.Abstractions`, so the application layer depends on an interface, not a
concrete store. The defect is narrower and still real: that interface is **in-process by
construction**. `TryGetValue` is synchronous, and eviction is expressed as a
`CancellationChangeToken` — a process-local primitive with no distributed equivalent.

A use case should state *what* it needs cached, not that the cache is in this process and evicted by
a token. Because the handler states the latter, moving to Redis is not a registration change; it
rewrites four handlers, replaces the eviction model, and turns two synchronous calls into awaits.

### 3. `IMemoryCache` is per-process

A like on instance A leaves instance B serving a stale ranking for up to ten minutes;
`CacheInvalidator.Invalidate()` cancels a `CancellationTokenSource` that only exists in the
process that ran the write. This is the known Stage 10 finding.

### 4. The best caching candidates are not cached at all

`LookupRepository` reads reference data from Postgres on **every** request:

```csharp
// src/Modules/Content/Content/Infrastructure/Repositories/LookupRepository.cs
public async Task<IReadOnlyList<ContentTypeEntity>> GetActiveContentTypesAsync(CancellationToken ct = default)
{
    var specification = new ActiveContentTypeSpecification();
    return await context.ContentTypes.ApplySpecification(specification).OrderBy(x => x.Name).ToListAsync(ct);
}
```

`ContentTypes`, `PricingTiers`, `PromotionLevels` and categories are tens of rows, mutated by an
admin a handful of times a year, and read constantly. They are exactly the `ICacheable` shape from
`dotnet-microservices`, with zero caching applied.

### 5. No stampede protection

`TryGetValue` → miss → query → `Set` has no coordination. Ten simultaneous requests for a cold
popular-articles feed run the ranking query ten times. On a cache expiry under load — exactly when
the feed is hottest — the cost of a miss is multiplied by concurrency.

### 6. Repository interfaces are unmanageably wide

Not a caching problem — the caching design never touches a repository — but a real one on its own.

| Interface | Lines | Implementation | Public members |
| --- | --- | --- | --- |
| `IArticleRepository` | 495 | 842 | ~39 |
| `IVideoRepository` | 307 | 464 | ~24 |
| `ILookupRepository` | 269 | 282 | 26 |
| `ILyricsRepository` | 257 | 408 | ~22 |

`ILookupRepository` is four unrelated aggregates in one contract — `ContentTypeEntity`,
`PricingTierEntity`, `PromotionLevelEntity`, `TagEntity` — so a handler that needs one content type
depends on 22 methods about pricing tiers and tags. Compare `IBasketRepository` in the EShop
reference: four members.

### 7. No repository base and no `Include` seam

Every repository re-implements `AddAsync`, `Remove`, `Update`, `GetByIdOrThrowAsync`, and every
finder repeats its own `Include` chain — there is no `Query()` override to declare the hydration
graph once. `ArticleRepository` has 49 public members across articles, comments, likes, bookmarks,
shares, tags and images.

## Why the commit boundary rules out a repository decorator

`EShopModularMonoliths` evicts write-through because the repository owns `SaveChangesAsync`. **This
backend cannot**, because the commit is a separate collaborator:

```csharp
// src/Modules/Content/Content/Infrastructure/Persistence/ContentUnitOfWork.cs
public class ContentUnitOfWork(ContentDbContext context) : IContentUnitOfWork
{
    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }
}
```

A repository decorator here would see `Update(entity)` and never learn whether the transaction
committed. That is not a limitation to work around — it is the reason invalidation belongs on the
post-commit domain events this codebase already dispatches, which is strictly more correct than
EShop's model (it evicts inside a `SaveChangesAsync` an outer transaction can still roll back).

Keep the event handlers; replace only what they call.

## Not treated as problems

- **`.AsTracking()` on write-path reads.** Stage 9's audit partitioned every repository into read and
  write methods. Under the current design that partition no longer has a caching job to do — nothing
  caches an entity — but it remains the right marker for which reads may be mutated.
- **The absence of a `Core.Contracts` project.** Content depending on `Core.Application` is a
  sideways module dependency the project graph permits; the boundary that matters, `Infrastructure`,
  is intact.
