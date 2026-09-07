# Stage 10 — Caching, concurrency and data access

Four concerns bundled by decision, not by theme: how the application **caches** (Part A), how it
behaves with **more than one process** (Parts B and C), and how it **reads and writes through
repositories** (Part D). The previous title, *Multi-instance readiness*, stopped describing the
stage once the caching rewrite and the repository contracts landed here — neither is multi-instance
work, and a reader looking for "why did `ArticleRepository` change?" would never have opened this
file.

Closes **[04 §8]** (Critical), **[04 §9]** (High), **[04 §10]** (High), **[04 §11]** (Medium),
**[04 §14]** (Medium), **[01 §1.9]**, **[01 §1.12]**, **[01 §1.13]**, **[01 §1.15]**, and all of
**[16]**.

**Part A — caching.** Four handlers each carry a private read-through block that has already
diverged `[16 §16.1]`; there is no stampede protection `[16 §16.2]`; the lookup tables and 22 other
read paths are cached nowhere `[16 §16.3]`; and the cache technology is welded into the use cases,
so moving stores rewrites handlers `[16 §16.4]`. Eviction runs through an in-process
`CancellationTokenSource` that a second instance cannot see `[04 §8]`.

**Parts B and C — more than one process.** Four Quartz jobs (`ExpiredOtpCleanupJob`,
`AbandonedDraftCleanupJob`, `ShortVideoViewEventCleanupJob`, `OutboxEmailDispatcherJob`) use the
in-memory store, so two instances run every job twice, concurrently `[04 §8]`. `ContentModule` runs
`ContentTypeSeeder.SeedAllAsync().GetAwaiter().GetResult()` during registration; two cold starts
race the `AnyAsync` check and double-insert — and the short-circuit (`if (alreadySeeded) return;`)
means a database seeded before `Lyrics` was added **never receives it** `[04 §9]`.
`UseMigration<TContext>()` calls `Database.MigrateAsync()` inside pipeline construction, so two
instances race the migration lock and Stage 9's `CREATE INDEX CONCURRENTLY` cannot run there at all
`[04 §10]`. The three `AddDbContextPool` registrations configure no retry, no command timeout and no
pool cap `[04 §14]`.

**Part D — data access.** `IRepository<T>` is an empty marker inherited by 26 interfaces `[01 §1.9]`;
33 repositories across 6,195 lines hand-write the same members; and `ArticleRepository` spreads 24
`Include` calls over 49 members, so hydration depends on which finder was called `[04 §11]`.

> Draft — finalized against the tree Stage 9 lands on. All types below verified against the
> current tree.

---

## Decisions

| # | Question | Options weighed | Decision |
| --- | --- | --- | --- |
| D1 | Cross-instance invalidation | Redis pub/sub, hand-built version keys, or `HybridCache` tags | **`HybridCache` tags.** The eviction-token pattern cannot be distributed — a `CancellationTokenSource` is process-local by nature. A hand-built version key would work, but `RemoveByTagAsync` is the same idea shipped by Microsoft, and it also brings stampede protection and L1+L2 in one API. This **supersedes the version-key remedy in `[04 §4.8]`** `[16 §16.4]`. |
| D2 | L1, L2, or both | Redis-only, or L1 memory + L2 Redis | **Both, via `HybridCache`.** The earlier draft chose Redis-only because L1 was what created the coherence bug — but the bug was *unevictable* L1, not L1 itself. `RemoveByTagAsync` reaches both layers, so keeping L1 costs nothing and saves a network hop per hit. `AddMemoryCache()` stays; `HybridCache` uses it as L1. |
| D3 | Job coordination | Quartz JDBC clustering, or advisory locks per job | **Quartz clustering.** The jobs are already Quartz (`AddScheduledJob<TJob>` → `AddQuartz`); pointing the scheduler at a persistent clustered store fixes all four at the registration seam, with none of the per-job lock code an advisory-lock approach needs. |
| D4 | Seeding | keep registration-time seeding + lock, or a hosted service | **Hosted service + advisory lock + per-item idempotency.** Seeding during `RegisterModule` blocks DI construction on I/O and cannot be ordered after migrations move. `IHostedService` runs after the host is built; `pg_advisory_lock` serializes replicas; upsert-per-row replaces the all-or-nothing `AnyAsync` short-circuit so the missing `Lyrics` row heals itself. |
| D5 | Dead seeder infra | wire `IDataSeeder`/`UseSeed`, or delete it | **Wire it.** The hosted service enumerates `IEnumerable<IDataSeeder>` — the interface finally earns its registration, and `ContentTypeSeeder` implements it instead of being manually resolved. `UseSeed()` and `SeedDataAsync` are deleted. |
| D6 | Where the cache lives | in the handlers, behind a repository decorator, or behind a handler decorator | **Decorator over `IRequestHandler<,>`** — the fourth in the chain already registered in `CqrsExtension.cs:62-64`. A repository decorator cannot work here: commit belongs to `IContentUnitOfWork`, so a repository never learns whether the transaction succeeded, and caching a repository result means serializing an aggregate. The cached value is the query result record the endpoint already serializes `[16 §16.4]`. |
| D7 | Migrations | init-container/CLI step, or keep startup migration behind a lock | **Explicit step.** `dotnet run --project src/Api -- migrate` (and the compose/deploy pipeline calls it) applies and exits. `EnableMigrations` defaults to `false` outside Development. Startup migration is what blocks Stage 9's `CONCURRENTLY` indexes and what makes deploys apply 60+ destructive operations implicitly `[04 §10]`. |

---

## Checklist

- [x] 10.1 — `Microsoft.Extensions.Caching.Hybrid`; `ICacheableRequest`; `CachingDecorator<,>`; `ContentCacheTags`
- [x] 10.2 — The four cached queries opt in; their handlers drop `IMemoryCache`, the invalidator and the TTL
- [x] 10.3 — Event handlers switch to `RemoveByTagAsync`; `ICacheInvalidator` and its five types deleted
- [x] 10.4 — Lookup tables cached (`content:lookups`, `identity:lookups`); admin mutations evict `[16 §16.3]`
- [x] 10.5 — The remaining 22 candidates from the census opt in (Groups 2 and 3); `IFileRepository` id→URL cache deferred to Stage 14.2b (§ deferral note)
- [x] 10.6 — Redis in compose/CI; `AddStackExchangeRedisCache` (L2); Testcontainers Redis in the fixture
- [x] 10.7 — Quartz JDBC store, clustered, `[DisallowConcurrentExecution]` on the four jobs
- [x] 10.8 — Seeding: `IDataSeeder` wired, advisory-locked hosted service, per-row idempotent upserts (heals `Lyrics`)
- [x] 10.9 — `migrate` entry point; `EnableMigrations=false` outside Development; `UseMigration` deleted
- [x] 10.10 — `EnableRetryOnFailure` + `CommandTimeout(30)` + pool size on `ConfigureDbContextOptions`
- [x] 10.11 — Dockerfile copies the Mailer projects
- [x] 10.12 — Tests: tag eviction visible across two instances; stampede runs the factory once; seeder run twice inserts once
- [x] 10.13 — `IReadRepository`/`IWriteRepository`/`IRepository<TEntity, TId>` replace the empty marker `[01 §1.9]`
- [x] 10.14 — `RepositoryBase<TContext, TEntity, TId>` + the three per-module bases
- [x] 10.15 — 33 repositories derive; the duplicated members deleted `[04 §11]`
- [x] 10.16 — `Query()` overrides for the aggregates with a hydration graph `[04 §11]` — seam shipped; blanket sweep rejected (§10.16 deviation)
- [x] 10.17 — `ILookupRepository` split into four; `ArticleRepository` split along aggregate lines `[04 §11]`
- [x] 10.18 — Verify (build 0/0, csharpier, unit, integration; `docker build` succeeds)

---

## Part A — Caching

### 10.1 The decorator, the marker and the tags

Caching moves out of the four handlers into one decorator, joining the three already registered in
`CqrsExtension.cs:62-64`. `HybridCache` is L1 (memory) + L2 (Redis) behind one API, with tag
invalidation and stampede protection built in.

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
    /// Article feeds and article detail projections.
    /// </summary>
    public const string Articles = "content:articles";

    /// <summary>
    /// Video feeds and video detail projections.
    /// </summary>
    public const string Videos = "content:videos";

    /// <summary>
    /// Lyrics pages, translations and revision listings.
    /// </summary>
    public const string Lyrics = "content:lyrics";

    /// <summary>
    /// Short-video feeds and detail projections.
    /// </summary>
    public const string Shorts = "content:shorts";

    /// <summary>
    /// Artist listings, profiles, articles and releases.
    /// </summary>
    public const string Artists = "content:artists";

    /// <summary>
    /// The lookup tables: content types, pricing tiers, promotion levels, categories.
    /// </summary>
    public const string Lookups = "content:lookups";
}
```

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

Registration — caching outermost, so a hit skips validation and logging:

```csharp
// src/Shared/Shared/Application/Extensions/CqrsExtension.cs
services.Decorate(typeof(IRequestHandler<,>), typeof(AccountRateLimitDecorator<,>));
services.Decorate(typeof(IRequestHandler<,>), typeof(ValidationDecorator<,>));
services.Decorate(typeof(IRequestHandler<,>), typeof(LoggingDecorator<,>));
services.Decorate(typeof(IRequestHandler<,>), typeof(CachingDecorator<,>));
```

```csharp
// src/Api/Program.cs — AddMemoryCache() stays; HybridCache uses it as L1.
builder.Services.AddHybridCache(options =>
{
    options.DefaultEntryOptions = new HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromMinutes(10),
        LocalCacheExpiration = TimeSpan.FromMinutes(10),
    };
});
```

> **Position in the chain is a decision, not line order.** Registered last, caching is outermost: a
> hit costs nothing but also skips validation and never appears in the request log. That is
> acceptable only because the key derives from an already-shaped query. Registering it before
> `LoggingDecorator` instead keeps hits observable at the cost of running the log path on every hit.
> **Decision: outermost**, on the grounds that a cached public feed is the hot path and its hits are
> better measured by cache metrics than by request logs. Revisit if hit-rate debugging needs it.

> **Package version.** The 9.x line of `Microsoft.Extensions.Caching.Hybrid` targets `net9.0`;
> 10.x targets .NET 10. Check the current 9.x release at implementation time rather than copying a
> version from this spec.

### 10.2 The four queries opt in; their handlers shed the caching

```csharp
// …/GetPopularArticles/PublicGetPopularArticlesQuery.cs
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

`PublicGetAllTagsQuery` is the one that already diverged `[16 §16.1]` — its `cacheable` guard becomes
a property rather than a branch inside the handler:

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

Each handler then loses `IMemoryCache`, its invalidator, its `CacheTtl` constant, the key
construction and the get/miss/set block — `PublicGetPopularArticlesHandler` goes from five
constructor dependencies to three and returns the repository result directly.

**Serialization constraint.** `HybridCache` serializes cached values so an entry can reach L2. Every
cached value here is a query result record — flat records of `Guid`, `string`, `int`, `bool`,
`DateTimeOffset?` and enums, already serialized on the wire by the endpoint. No converter, no
attribute. A result type that cannot round-trip through `System.Text.Json` must not be cached, which
is what keeps aggregates out of the cache by construction.

### 10.3 Invalidation becomes a tag; the invalidator types are deleted

The three event handlers keep their job — and their post-commit timing, which is the part this
codebase already gets right — and change one line each:

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

The Stage 8 engagement handlers (`ArticleEngagementHandler`, `VideoEngagementHandler`) call
`cacheInvalidator.Invalidate()` fire-and-forget today; they become
`await cache.RemoveByTagAsync(tag, cancellationToken)`. Their unit tests follow the signature.

Deleted once every consumer has moved — **in this order**, or the handlers lose eviction mid-series:

| Deleted | Path |
| --- | --- |
| `ICacheInvalidator` | `Application/Shared/Cache/` |
| `IPopularArticlesCacheInvalidator`, `IPopularVideosCacheInvalidator`, `IPopularTagsCacheInvalidator` | `Application/Shared/Cache/` |
| `CacheInvalidator` + its three subclasses | `Infrastructure/Cache/` |

Seven types, roughly 120 lines. Added: one decorator, two marker interfaces, one constants class.

### 10.4 The lookup tables gain caching `[16 §16.3]`

New capability, not a refactor. `ContentTypes`, `PricingTiers`, `PromotionLevels` and the category
reads hit Postgres on every request today and are mutated a few times a year. Each read query
implements `ICacheableRequest` with `ContentCacheTags.Lookups` and a 30-minute TTL:

```csharp
public record AdminGetAllContentTypesQuery(string? Search)
    : IQuery<AdminGetAllContentTypesResult>,
        IConditionallyCacheableRequest
{
    /// <inheritdoc />
    public bool IsCacheable => string.IsNullOrWhiteSpace(Search);

    /// <inheritdoc />
    public string CacheKey => "lookup:content_types:all";

    /// <inheritdoc />
    public TimeSpan Ttl => TimeSpan.FromMinutes(30);

    /// <inheritdoc />
    public IReadOnlyList<string> CacheTags => [ContentCacheTags.Lookups];
}
```

Eviction hangs off the admin mutation handlers, which already run inside the request that commits:

```csharp
await unitOfWork.CommitAsync(cancellationToken);
await cache.RemoveByTagAsync(ContentCacheTags.Lookups, cancellationToken);
```

> **Why not a domain event here.** `ContentTypeEntity`, `PricingTierEntity`, `PromotionLevelEntity`
> and `CategoryEntity` raise no domain events today, so event-driven eviction would mean adding four
> events and their handlers. For four admin-only mutation paths, evicting after the commit in the
> handler is the smaller correct change; revisit if these aggregates gain events for other reasons.

### 10.5 The full candidate census

Every read path in the solution, classified. 89 query handlers were enumerated (Content 70,
Identity 16, Mailer 3, Core 0) plus the two non-CQRS hot reads. **Cache exactly what is listed as
cacheable and nothing else** — the "never" list is as much a part of this stage as the rest.

#### Group 1 — reference data · tag `content:lookups` / `identity:lookups` · TTL 30 min

Mutated by an admin a handful of times a year, read on effectively every page. **None is cached
today.**

| Query | Module |
| --- | --- |
| `PublicGetAllContentTypesQuery`, `AdminGetAllContentTypesQuery` | Content |
| `PublicGetActivePromotionLevelsQuery`, `AdminGetAllPromotionLevelsQuery` | Content |
| `AdminGetAllPricingTiersQuery` | Content |
| `PublicGetActiveCategoriesQuery`, `PublicGetExclusiveCategoryQuery`, `AdminGetAllCategoriesQuery` | Content |
| `AdminGetAllRolesQuery`, `AdminGetRoleByIdQuery` | Identity |
| `AdminGetAllPermissionsQuery`, `AdminGetPermissionByIdQuery` | Identity |

Roles and permissions belong here for the same reason content types do: they change on a
deployment or an admin action, not on user traffic.

#### Group 2 — shared public feeds · per-domain tag · TTL 10 min

No user identity in the query, so one entry serves every caller. Four are cached today; **ten are
not.**

| Query | Tag | Cached today |
| --- | --- | --- |
| `PublicGetPopularArticlesQuery` | `content:popular-articles` | yes |
| `PublicGetPopularVideosQuery` | `content:popular-videos` | yes |
| `PublicGetPopularTagsQuery` | `content:tags` | yes |
| `PublicGetAllTagsQuery` | `content:tags` | yes (conditional) |
| `PublicGetPublishedVideosQuery` | `content:videos` | **no** |
| `PublicGetPromotedVideosQuery` | `content:videos` | **no** |
| `PublicGetVideoFeedQuery` | `content:videos` | **no** |
| `PublicGetVideoPromotionFeedQuery` | `content:videos` | **no** |
| `PublicGetArtistsQuery` | `content:artists` | **no** |
| `PublicGetArtistBySlugQuery` | `content:artists` | **no** |
| `PublicGetArtistArticlesQuery` | `content:artists` | **no** |
| `PublicGetArtistReleasesQuery` | `content:artists` | **no** |
| `PublicGetLyricsTranslationsQuery` | `content:lyrics` | **no** |
| `PublicGetTranslationRevisionsQuery` | `content:lyrics` | **no** |

The video feeds are the notable omission: `PublicGetPublishedVideosQuery` and
`PublicGetVideoFeedQuery` carry no user identity and are as hot as the popular feeds, but only the
*popular* variants were ever cached.

**Excluded from this group deliberately:** `PublicGetArticleCommentsQuery` and
`PublicGetCommentRepliesQuery`. They carry no user identity and would technically cache, but
comments are the highest-churn shared read in the module — a 10-minute entry means a user posts a
comment and cannot see it. Leave them uncached unless a TTL under 30 seconds proves worthwhile.

#### Group 3 — anonymous-only · same tags as Group 2 · TTL 10 min

Twelve queries carry `Guid? CurrentUserId = null` purely to stamp `IsLiked` / `IsBookmarked`. The
underlying projection is identical for every caller; only the flags differ. Cache **only the
anonymous case**:

```csharp
public record PublicGetPublishedArticlesQuery(int Page, int PageSize, Guid? CategoryId, Guid? CurrentUserId = null)
    : IQuery<PublicGetPublishedArticlesResult>,
        IConditionallyCacheableRequest
{
    /// <inheritdoc />
    /// <remarks>
    /// Only the anonymous projection is stored: an authenticated response carries per-user
    /// interaction flags, and caching it would show one reader another reader's likes.
    /// </remarks>
    public bool IsCacheable => CurrentUserId is null;

    /// <inheritdoc />
    public string CacheKey => $"published_articles:{Page}:{PageSize}:{CategoryId?.ToString() ?? "all"}";

    /// <inheritdoc />
    public TimeSpan Ttl => TimeSpan.FromMinutes(10);

    /// <inheritdoc />
    public IReadOnlyList<string> CacheTags => [ContentCacheTags.Articles];
}
```

| Query |
| --- |
| `PublicGetPublishedArticlesQuery`, `PublicGetPromotedArticlesQuery`, `PublicGetArticleBySlugQuery` |
| `PublicGetArticlePromotionFeedQuery` |
| `PublicGetVideoBySlugQuery` |
| `PublicGetPublishedLyricsQuery`, `PublicGetLyricsBySlugQuery`, `PublicGetLyricsByVideoIdQuery`, `PublicGetSimilarLyricsQuery` |
| `PublicGetPublicShortsQuery`, `PublicGetPublicShortBySlugQuery`, `PublicGetShortsFeedQuery` |

> **A better version exists, and is deliberately deferred.** `ArticleMapper` already layers
> `ToArticleSummaryDtosAsync(mapper, fileRepository, likedIds, bookmarkedIds, ct)` over the flagless
> overload, so the flagless projection could be cached once and the flags stamped per request —
> serving authenticated traffic from cache too. That needs the handler split into
> cached-projection + per-request stamping, which is a larger change than the marker interface.
> Do the anonymous case first; revisit if the hit rate on authenticated traffic matters.

#### Group 4 — non-CQRS hot reads

Two read paths that no query handler owns, so the decorator cannot reach them. Both need their own
treatment.

**`IFileRepository.GetByIdsAsync` — 12 call sites.** Every list mapper resolves cover-image URLs
through it, so it runs on nearly every content response. `FileEntity` is immutable after upload
apart from `Delete()` and `MarkReplaced()`, both of which raise domain events. Cache the id → URL
resolution behind the repository (this is the one place a repository decorator *is* right — the
value is a `string`, not an aggregate), evicted by `FileSoftDeletedEvent`.

**`IFileRepository.GetByIdsAsync` — deferral recorded at implementation time.** The method returns
`FileEntity` aggregates, so caching it today would either serialize an aggregate (forbidden by the
rule above) or require a string-returning resolver contract mid-stage. That contract is exactly
Stage 14.2's `IFileStore`/`FileRef` — the cache lands there, one decorator over the new interface.

**`AccountStatusRequirementHandler` — one `SELECT` per request across 150 endpoints.** It loads the
user on every `RequireActiveUser` call. `[07 §S12]` already prescribes caching it with a 30–60s TTL,
and `UserSecurityStateCache` (5-minute TTL) is the existing pattern to follow. **This belongs to
Stage 11**, which rewrites the same method to fail closed — do not touch it here, or the two stages
edit the same lines twice.

#### Never cached

| Read | Count | Why |
| --- | --- | --- |
| `GetOwn*` queries — bookmarks, likes, shares, comments, playlists, rated videos, profile, roles, sessions | 20 | Per-user; the key space is users × items and the value is wrong for everyone else |
| `PublicGetNotificationsQuery`, `PublicGetUnreadNotificationCountQuery` | 2 | Per-user and the highest-churn read in Mailer |
| Session queries and `AdminGetSessionMetricsQuery`, `AdminExportSessionDataQuery` | 5 | Per-user, security-sensitive, and metrics must be live |
| Order, payment and customer reads | 6 | Money. A stale payment state is a support incident |
| Admin lists carrying a free-text `search` | — | Unbounded key space; `IsCacheable` returns false |
| `PublicGetArticleCommentsQuery`, `PublicGetCommentRepliesQuery` | 2 | Highest-churn shared read; a poster must see their own comment |
| Anything returning an aggregate | — | It would have to serialize for L2 |

#### Tag summary

Every tag used by this stage, and what evicts it:

| Tag | Evicted by |
| --- | --- |
| `content:popular-articles` | `ArticlePublishedEvent`, `ArticleUnpublishedEvent`, `ArticleDeletedEvent`, `ArticleEngagedEvent` |
| `content:popular-videos` | the `Video*` equivalents, `VideoEngagedEvent` |
| `content:articles` | the article publish-state events (Group 3 entries) |
| `content:videos` | the video publish-state events (Group 2 and 3 entries) |
| `content:lyrics` | `LyricsRevisionDecidedEvent`, `TranslationRevisionDecidedEvent`, lyrics publish-state changes |
| `content:artists` | `ArtistOwnershipVerifiedEvent`, artist and album admin mutations |
| `content:shorts` | short-video publish-state and deletion events |
| `content:tags` | `TagGraphChangedEvent` |
| `content:lookups` | content-type, pricing-tier, promotion-level and category admin mutations |
| `identity:lookups` | role and permission admin mutations |

Tags that have no domain event today — `content:lookups`, `identity:lookups`, `content:artists` —
are evicted from the admin mutation handler after `CommitAsync`, per the note in 10.4.

### 10.6 Redis becomes L2

Everything above runs L1-only and is already a strict improvement — stampede protection and tag
eviction work in-process from the first commit. This step adds the second layer and is the only one
that needs infrastructure:

```csharp
// src/Api/Program.cs — registered before AddHybridCache; HybridCache picks it up as L2.
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "116:";
});
```

`Microsoft.Extensions.Caching.StackExchangeRedis` is already a `PackageReference` in
`Shared.csproj:31`, so this is compose, CI, the Testcontainers fixture and one registration — no
code change anywhere else.

> **Finding at implementation time — `HybridCache` tags do not evict warm instances.** Verified
> against the 9.10 and 10.x sources and proven by test: each instance caches a tag's
> invalidation timestamp in-process on first touch and never re-reads it
> (`_tagInvalidationTimes` is `TryAdd`-only). `RemoveByTagAsync` on instance B therefore never
> reaches an instance A that has already served that tag — A serves stale entries until their
> own TTL, which is exactly the `[04 §8]` staleness this stage exists to close.
>
> **Fix: `BackplaneHybridCache`** — a decorator over `HybridCache` (Scrutor, same seam as
> everything else) that publishes every `RemoveByTagAsync`/`RemoveAsync` on a Redis pub/sub
> channel and replays received evictions through the inner cache, which refreshes the local tag
> view. Registered only when `REDIS_URL` is set; single-instance and test hosts run undecoated.
> `CacheCoherenceTests.RemoveByTag_OnOneInstance_ShouldReachAWarmInstanceThroughTheBackplane`
> fails without it and passes with it.

## Part B — Jobs and seeding

### 10.7 Clustered Quartz

`QuartzExtension.AddScheduledJob<TJob>` keeps its signature; the store configuration moves into
the first-call setup:

```csharp
services.AddQuartz(q =>
{
    q.UsePersistentStore(store =>
    {
        store.UsePostgres(postgres =>
        {
            postgres.ConnectionString = connectionString;
            postgres.TablePrefix = "quartz.qrtz_";
        });
        store.UseClustering();
        store.UseNewtonsoftJsonSerializer();
    });

    var jobKey = new JobKey(typeof(TJob).Name);
    q.AddJob<TJob>(opts => opts.WithIdentity(jobKey));
    q.AddTrigger(opts =>
        opts.ForJob(jobKey).WithIdentity($"{typeof(TJob).Name}-trigger").WithCronSchedule(cronExpression)
    );
});
```

The four job classes get `[DisallowConcurrentExecution]` so a slow run and its successor cannot
overlap even on one instance. The Quartz schema ships as a migration in `quartz` schema
(generated, left unapplied per house rule).

### 10.8 Seeding

`ContentTypeSeeder.SeedAllAsync` today short-circuits on `AnyAsync` — which is both the race and
the reason `Lyrics` never seeds into an old database. It becomes an `IDataSeeder` with per-row
idempotency:

```csharp
/// <inheritdoc />
public async Task SeedAsync(CancellationToken cancellationToken = default)
{
    foreach (string name in ContentTypeNames)
    {
        bool exists = await context.ContentTypes.AnyAsync(t => t.Name == name, cancellationToken);
        if (exists)
        {
            continue;
        }

        context.ContentTypes.Add(ContentTypeEntity.Create(name: name));
    }

    await context.SaveChangesAsync(cancellationToken);
}
```

One hosted service replaces every registration-time `GetAwaiter().GetResult()` seeding call
(`ContentModule.cs:269` and its Identity siblings):

```csharp
namespace _116.Shared.Infrastructure.Seeding;

/// <summary>
/// Runs every registered <see cref="IDataSeeder" /> once per deployment under a Postgres
/// advisory lock, so N starting replicas seed exactly once between them.
/// </summary>
public class DataSeedingHostedService(IServiceProvider serviceProvider, ILogger<DataSeedingHostedService> logger)
    : IHostedService
{
    private const long SeedLockKey = 116_001;

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        var connection = new NpgsqlConnection(scope.ServiceProvider.GetRequiredService<IConfiguration>()
            .GetConnectionString("Database"));
        await connection.OpenAsync(cancellationToken);

        await using (NpgsqlCommand acquire = new($"SELECT pg_advisory_lock({SeedLockKey})", connection))
        {
            await acquire.ExecuteNonQueryAsync(cancellationToken);
        }

        try
        {
            foreach (IDataSeeder seeder in scope.ServiceProvider.GetServices<IDataSeeder>())
            {
                logger.LogInformation("Seeding via {Seeder}.", seeder.GetType().Name);
                await seeder.SeedAsync(cancellationToken);
            }
        }
        finally
        {
            await using NpgsqlCommand release = new($"SELECT pg_advisory_unlock({SeedLockKey})", connection);
            await release.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
```

`UseSeed()`/`SeedDataAsync` in `ApplicationBuilderExtension` are deleted `[01 §1.13]`.

### 10.9 Migrations out of the pipeline

`UseMigration<TContext>()` (which does `MigrateDatabaseAsync(...).GetAwaiter().GetResult()`
inside pipeline construction) is deleted. `Program.cs` grows an explicit mode before
`builder.Build()`'s pipeline is wired:

```csharp
if (args.Contains("migrate"))
{
    using IServiceScope scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<CoreDbContext>().Database.MigrateAsync();
    await scope.ServiceProvider.GetRequiredService<IdentityDbContext>().Database.MigrateAsync();
    await scope.ServiceProvider.GetRequiredService<ContentDbContext>().Database.MigrateAsync();
    await scope.ServiceProvider.GetRequiredService<MailerDbContext>().Database.MigrateAsync();
    return;
}
```

`ModuleOptions.EnableMigrations` defaults to `false`; Development opts in explicitly so the local
loop keeps working. Deploy order becomes: run `migrate` (as a job/init container), then roll the
instances — which is also the only order under which Stage 9's `CONCURRENTLY` indexes can build.

## Part C — Resilience and the build

### 10.10 EF resilience

`BaseModule.ConfigureDbContextOptions` (the single seam all three pooled contexts share):

```csharp
options
    .UseNpgsql(connectionString, npgsql =>
    {
        npgsql.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorCodesToAdd: null);
        npgsql.CommandTimeout(30);
    })
    .UseSnakeCaseNamingConvention();
```

> `EnableRetryOnFailure` installs a retrying execution strategy: code that opens explicit
> transactions must wrap them in `CreateExecutionStrategy().ExecuteAsync(...)`. Nothing in
> Content/Core opens one today (`[04 §7]` — that's the Stage 13 work), so land this **before**
> Stage 13 and let 13 build on the strategy, not fight it.

### 10.11 Dockerfile

The build stage copies every module's projects but Mailer's — the container restore fails from a
clean context `[01 §1.15]`. Add the missing pair alongside the existing module copies:

```dockerfile
COPY ["src/Modules/Mailer/Mailer/Mailer.csproj", "src/Modules/Mailer/Mailer/"]
COPY ["src/Modules/Mailer/Mailer.Contracts/Mailer.Contracts.csproj", "src/Modules/Mailer/Mailer.Contracts/"]
```

(Exact project list from the current `.sln` at implementation time.)

---

## Part D — Repository contracts

Unrelated to multi-instance correctness and grouped here by decision: it is the other half of the
[repository-and-caching study](../../repository-and-caching/06-repository-contracts.md), and doing
it in the same stage keeps that study in one PR series.

> **Review note.** Parts A–C are infrastructure; Part D is a 33-file refactor. Keep it as its own
> commit series so it can be split into a second PR if the combined diff is too large to review.

**The coverage map previously routed `[04 §11]` to Stage 15, which never specified it** — Stage 15
covers aggregate boundaries and specifications, not repository shape. That was a planning bug; the
finding lands here.

### 10.13 The contract

`_116.Shared.Domain.IRepository<T>` is an empty marker inherited by 26 interfaces that adds nothing
`[01 §1.9]`. Replace it with three role interfaces:

```csharp
// src/Shared/Shared/Domain/IRepository.cs
namespace _116.Shared.Domain;

/// <summary>
/// Identity reads and existence probes for an aggregate. Results are untracked; a caller that
/// intends to mutate uses <see cref="IWriteRepository{TEntity, TId}" />.
/// </summary>
/// <typeparam name="TEntity">The aggregate type.</typeparam>
/// <typeparam name="TId">The aggregate's identifier type.</typeparam>
public interface IReadRepository<TEntity, TId>
    where TEntity : class, IEntity<TId>
    where TId : struct
{
    /// <summary>
    /// Reads an aggregate by identity, or null when none exists.
    /// </summary>
    /// <param name="id">The aggregate identifier.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The aggregate, or null.</returns>
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reports whether an aggregate with the supplied identity exists.
    /// </summary>
    /// <param name="id">The aggregate identifier.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>True when the aggregate exists.</returns>
    Task<bool> ExistsAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Throws when no aggregate with the supplied identity exists.
    /// </summary>
    /// <param name="id">The aggregate identifier.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <exception cref="NotFoundException">Thrown when no aggregate has the supplied id.</exception>
    Task ExistsOrThrowAsync(TId id, CancellationToken cancellationToken = default);
}

/// <summary>
/// Mutation of an aggregate: the tracked load mutation requires, and the staging calls the unit of
/// work later commits. Commit itself belongs to <see cref="IUnitOfWork" />, never here.
/// </summary>
/// <typeparam name="TEntity">The aggregate type.</typeparam>
/// <typeparam name="TId">The aggregate's identifier type.</typeparam>
public interface IWriteRepository<TEntity, TId>
    where TEntity : class, IEntity<TId>
    where TId : struct
{
    /// <summary>
    /// Reads a tracked aggregate by identity for mutation.
    /// </summary>
    /// <param name="id">The aggregate identifier.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The tracked aggregate.</returns>
    /// <exception cref="NotFoundException">Thrown when no aggregate has the supplied id.</exception>
    Task<TEntity> GetByIdOrThrowAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stages a new aggregate for insertion on the next commit.
    /// </summary>
    /// <param name="entity">The aggregate to add.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stages an existing aggregate for update on the next commit.
    /// </summary>
    /// <param name="entity">The aggregate to update.</param>
    void Update(TEntity entity);

    /// <summary>
    /// Stages an aggregate for deletion on the next commit.
    /// </summary>
    /// <param name="entity">The aggregate to remove.</param>
    void Remove(TEntity entity);
}

/// <summary>
/// The union of both roles, for aggregates whose repository serves read and write paths alike.
/// </summary>
/// <typeparam name="TEntity">The aggregate type.</typeparam>
/// <typeparam name="TId">The aggregate's identifier type.</typeparam>
public interface IRepository<TEntity, TId> : IReadRepository<TEntity, TId>, IWriteRepository<TEntity, TId>
    where TEntity : class, IEntity<TId>
    where TId : struct;
```

Three roles rather than one so a guard-only client — the 18 interaction handlers converted to
`ExistsOrThrowAsync` in Stage 9 — can depend on `IReadRepository<ArticleEntity, Guid>` and cannot
reach `Remove`.

Three deliberate departures from the `dotnet-microservices` contract this is modelled on:

| Their contract | Here | Why |
| --- | --- | --- |
| `SaveChangesAsync` on the repository | absent | `IContentUnitOfWork.CommitAsync()` owns commit so a handler can mutate three aggregates and commit once |
| `IQueryable Query()` / `QueryNotTracked()` public | `protected` | a public `IQueryable` lets handlers compose EF expressions, defeats the specification pattern, and makes every repository unfakeable |
| `GetById` → `Single()` → `InvalidOperationException` | `FirstDefaultOrThrowAsync` → `NotFoundException` | the localized 404 contract already exists |

> **Deviation at implementation time — one contract, no role split.** The sketch below defines
> `IReadRepository`/`IWriteRepository` roles that repository interfaces would extend. That
> consumer side never materialised: call sites use named arguments
> (`ExistsOrThrowAsync(articleId: …)`), so inherited declarations would rename every parameter to
> `id` and break them — the 26 interfaces therefore keep their own declarations, and the role
> interfaces would have had zero consumers. They were collapsed into a single
> `IRepository<TEntity, TId>` that `RepositoryBase` implements: the contract constrains what
> every derived repository must provide, and nothing unused ships. The duplication lived in the
> bodies, and that is what was deleted.
>
> **Deviation at implementation time — the module bases are consumable, not just inheritable.**
> Auditing the reference solution showed its load-bearing pattern is not the interface (its
> `IRepository<T>` also has zero injection sites) but the generic repository closed over the
> service's context, registered open-generic and injected directly into handlers
> (`Repository<Person>`). The backend adopts that consumability behind an interface: each
> module declares `I<Module>Repository<TEntity> : IRepository<TEntity, Guid>` in
> `Application/Shared/Repositories`, the concrete `<Module>Repository<TEntity>` implements it,
> and DI maps interface to implementation open-generic
> (`services.AddScoped(typeof(IContentRepository<>), typeof(ContentRepository<>))`). A
> consumer needing only the seven common operations injects the closed interface — a new
> aggregate gets full CRUD with zero boilerplate, and unit tests can mock it. A single
> `IRepository<>` mapping is impossible here because four DbContexts would make the
> implementation ambiguous; the per-module interface carries the module binding. Integration
> coverage resolves `IContentRepository<TagEntity>` from DI and exercises the round trip
> against Postgres.

### 10.14 The base and the hydration seam

```csharp
// src/Shared/Shared/Infrastructure/Repositories/RepositoryBase.cs
namespace _116.Shared.Infrastructure.Repositories;

/// <summary>
/// Entity Framework implementation of <see cref="IRepository{TEntity, TId}" />. Derived types
/// override <see cref="Query" /> to declare the aggregate's hydration graph once.
/// </summary>
/// <typeparam name="TContext">The module database context.</typeparam>
/// <typeparam name="TEntity">The aggregate type.</typeparam>
/// <typeparam name="TId">The aggregate's identifier type.</typeparam>
/// <param name="context">The module database context.</param>
public abstract class RepositoryBase<TContext, TEntity, TId>(TContext context) : IRepository<TEntity, TId>
    where TContext : DbContext
    where TEntity : class, IEntity<TId>
    where TId : struct
{
    /// <summary>
    /// The module database context.
    /// </summary>
    protected TContext Context => context;

    /// <summary>
    /// The query root carrying every navigation this aggregate's reads need. Tracking follows the
    /// module default; override to add <c>Include</c> calls rather than repeating them per finder.
    /// </summary>
    protected virtual IQueryable<TEntity> Query() => context.Set<TEntity>();

    /// <summary>
    /// The tracked query root, carrying the same hydration graph as <see cref="Query" />. Every
    /// read whose result will be mutated goes through this.
    /// </summary>
    protected IQueryable<TEntity> QueryTracked() => Query().AsTracking();

    /// <inheritdoc />
    public virtual async Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        return await Query().FirstOrDefaultAsync(entity => entity.Id.Equals(id), cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<TEntity> GetByIdOrThrowAsync(TId id, CancellationToken cancellationToken = default)
    {
        return await QueryTracked()
            .Where(entity => entity.Id.Equals(id))
            .FirstDefaultOrThrowAsync(keyValue: id, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<bool> ExistsAsync(TId id, CancellationToken cancellationToken = default)
    {
        return await context.Set<TEntity>().AnyAsync(entity => entity.Id.Equals(id), cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task ExistsOrThrowAsync(TId id, CancellationToken cancellationToken = default)
    {
        bool exists = await ExistsAsync(id: id, cancellationToken: cancellationToken);

        if (!exists)
        {
            throw new NotFoundException(typeof(TEntity).Name, id);
        }
    }

    /// <inheritdoc />
    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await context.Set<TEntity>().AddAsync(entity, cancellationToken);
    }

    /// <inheritdoc />
    public virtual void Update(TEntity entity)
    {
        context.Set<TEntity>().Update(entity);
    }

    /// <inheritdoc />
    public virtual void Remove(TEntity entity)
    {
        context.Set<TEntity>().Remove(entity);
    }
}
```

**The tracking seam is inverted** relative to the reference — `Query()` follows the module default
and `QueryTracked()` is the explicit opt-in — because `ContentModule` sets
`UseNoTrackingByDefault = true` and Stage 9 made `.AsTracking()` the marker on write paths. Identity
and Core do not opt in, so the base must not assume either default.

**Verify on the first derived repository:** `entity.Id.Equals(id)` in an expression tree relies on EF
translating `Equals` to SQL equality. It does for `Guid`, and the reference base uses the same form,
but confirm it once rather than across 33 conversions. Adding `where TId : struct, IEquatable<TId>`
documents the intent and selects the typed overload.

One per-module closure, so entity repositories name one type parameter instead of three:

```csharp
// src/Modules/Content/Content/Infrastructure/Repositories/ContentRepository.cs
/// <summary>
/// Base repository for Content aggregates keyed by <see cref="Guid" />.
/// </summary>
/// <param name="context">The Content module database context.</param>
public abstract class ContentRepository<TEntity>(ContentDbContext context)
    : RepositoryBase<ContentDbContext, TEntity, Guid>(context)
    where TEntity : class, IEntity<Guid>;
```

`IdentityRepository<TEntity>` and `CoreRepository<TEntity>` are the same one-liner.

### 10.15 The 33 repositories derive

30 `AddAsync`, 55 `Update`, 15 `Remove` and 13 `GetByIdOrThrowAsync` implementations across
`src/Modules` collapse into the base. `AlbumRepository` goes from 107 lines / 6 members to ~60 / 2:

```csharp
/// <summary>
/// Implementation of <see cref="IAlbumRepository" /> for managing album entities.
/// </summary>
/// <param name="context">The Content module database context.</param>
public class AlbumRepository(ContentDbContext context) : ContentRepository<AlbumEntity>(context), IAlbumRepository
{
    // GetByIdAsync, GetByIdOrThrowAsync, AddAsync and Update are inherited.
    // Only GetAllAsync and GetByArtistAsync — the album-specific queries — remain.
}
```

**Mechanical but not blind.** Deleting `GetByIdOrThrowAsync` only works where the specification is a
plain id match. `AlbumByIdSpecification` and `ArtistByIdSpecification` are; check each, and where a
specification adds filtering keep the local implementation.

> **Deviation at implementation time — the seam ships, the sweep does not.** Hydration in this
> codebase is deliberately non-uniform per finder: `CategoryRepository` alone has six finders
> including only `ContentType`, by-id loads adding `Pricing.PricingTier`, and `GetBySlugAsync`
> including nothing. A per-aggregate `Query()` override would silently change every one of them.
> What landed: `Query()`/`QueryTracked()` exist as protected virtual seams on the base and every
> derived repository routes through them; the finders that need navigations keep them as explicit
> `override` members (Article, Category, Lyrics, Video by-id pairs). Blanket overrides are added
> per aggregate only when a consolidation need arises — the ArticleRepository class split in
> 10.17 is the natural first site.

### 10.16 `Query()` overrides — the reason to do this at all

`ArticleRepository` spreads 24 `Include` calls across 49 members — `Category` seven times,
`PromotionLevel` four, `Images` four, `Tags` three — so whether a loaded article carries its tags
depends on which finder was called. That is the class of bug behind `AddItemTierFactory` reading
`item.Tiers` from a method with no `Include`: it worked only because an earlier tracked load had
populated the identity map.

```csharp
public class ArticleRepository(ContentDbContext context)
    : ContentRepository<ArticleEntity>(context), IArticleRepository
{
    /// <inheritdoc />
    /// <remarks>
    /// Split into separate round-trips: three collection includes on one query multiply rows
    /// cartesian-wise.
    /// </remarks>
    protected override IQueryable<ArticleEntity> Query()
    {
        return base.Query()
            .Include(article => article.Category)
            .Include(article => article.PromotionLevel)
            .Include(article => article.Images)
            .Include(article => article.Tags)
            .AsSplitQuery();
    }
}
```

**This step is not behaviour-preserving** — finders that previously loaded a bare aggregate now load
its navigations, changing the generated SQL. Land it in its own commits, after 10.15 is green, and
re-read the Stage 9 split-query notes before touching `ArticleRepository`. Where a hot read wants a
leaner graph — the public feeds — that method calls `Context.Set<ArticleEntity>()` directly and says
why in a comment.

> **Status at implementation time — both halves done.** `ILookupRepository`: four interfaces,
> four implementations deriving `ContentRepository<T>`, four mock helpers, unit and integration
> repository tests split per aggregate, 31 src consumers and 31 test files converted.
> `ArticleRepository`: split into `IArticleRepository` (editorial core, 22 members),
> `IArticleCommentRepository` (14) and `IArticleInteractionRepository` (13) — both split
> interfaces declare `ExistsOrThrowAsync` so guard handlers depend only on their own role; the
> three classes share `ContentDbContext` through `ContentRepository<ArticleEntity>`, so the unit
> of work still commits them together. 19 handlers swapped to a narrow role, 7 mixed handlers
> carry two roles (the four public feeds pair core with interactions for flag stamping),
> `MockArticleRepository` split into three, and the unit/integration repository tests split per
> role. Editorial handlers were untouched. Verified green: unit 8,021/0, integration 2,029/2,029.

### 10.17 The two god-repositories split

`[04 §11]`'s remaining half. The base class removes the duplicated members; it does not make a
26-member interface spanning four aggregates any smaller.

**`ILookupRepository` → four.** It currently covers `ContentTypeEntity`, `PricingTierEntity`,
`PromotionLevelEntity` and `TagEntity` in one contract, so a handler that needs one content type
depends on 22 methods about pricing tiers and tags:

```csharp
public interface IContentTypeRepository : IRepository<ContentTypeEntity, Guid>
{
    /// <summary>
    /// Reads every content type, optionally narrowed by a free-text search term.
    /// </summary>
    /// <param name="search">Free-text term, or null for the full list.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The content types ordered by name.</returns>
    Task<IReadOnlyList<ContentTypeEntity>> GetAllAsync(
        string? search = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Reads the content types available for category assignment.
    /// </summary>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The active content types ordered by name.</returns>
    Task<IReadOnlyList<ContentTypeEntity>> GetActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Reports whether a content type already carries the supplied name.
    /// </summary>
    /// <param name="name">The name to probe.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>True when the name is taken.</returns>
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
}
```

`IPricingTierRepository`, `IPromotionLevelRepository` and `ITagRepository` follow the same shape.
Bodies move verbatim from `LookupRepository`; 34 files in `src/` and 34 in `tests/` update their
`using` and their injected type.

**`ArticleRepository` → three.** 842 lines and 49 members across articles, comments, likes,
bookmarks, shares, tags and images:

| Interface | Owns |
| --- | --- |
| `IArticleRepository` | the article aggregate, its images and its tag associations |
| `IArticleCommentRepository` | comments, replies, comment likes |
| `IArticleInteractionRepository` | likes, bookmarks, shares, the engagement counters |

All three take `ContentDbContext`, so `IContentUnitOfWork` still commits them together in one
transaction — the split changes no transactional boundary.

**Two parts of `[04 §11]` stay out of this stage**, because they belong to work already specified
elsewhere and duplicating them here would create conflicting instructions:

| `[04 §11]` clause | Stage | Why not here |
| --- | --- | --- |
| `FileRepository` upload orchestration → `FileUploadService` | **14.5** | Part of the file-pipeline rewrite, with the Polly client and atomic upload+write |
| query builders → `Infrastructure/Queries/` | **15** | It is the `Application → Infrastructure` layering violation `[06 §14]`, tracked with the specification cleanup |

---

## Tests

- **Integration — cache coherence:** two `HybridCache` instances over one Testcontainers Redis:
  an entry stored through A is served to B from the shared layer; a key removed through B is gone
  for A; and — the load-bearing one — a tag evicted through B reaches a **warm** A through the
  eviction backplane, which fails without `BackplaneHybridCache`.
- **Integration — the existing regression must pass unchanged:** like an article over HTTP, the
  popular feed no longer serves the stale count (`CacheInvalidationRegressionTests`). It is the
  proof that `RemoveByTagAsync` reaches the same entries the eviction token did.
- **Integration — seeder idempotency:** run `DataSeedingHostedService.StartAsync` twice against
  one database; content-type rows count once. Delete the `Lyrics` row, run again, assert it
  heals.
- **Integration — jobs:** `[DisallowConcurrentExecution]` present on all four job classes
  (reflection assertion), Quartz store config points at Postgres.
- **Unit — the decorator:** against a real `HybridCache` built from `AddHybridCache` in a
  `ServiceCollection`. A non-cacheable request always reaches the inner handler; a cacheable one
  reaches it once across two calls; `IsCacheable: false` always reaches it; a tag eviction between
  two calls forces a second inner call.
- **Unit — stampede `[16 §16.2]`:** N concurrent `Handle` calls on one cold key invoke the inner
  handler exactly once. This test fails against today's code and is the reason the finding exists.
- **Unit — keys:** every cacheable query's `CacheKey` is stable for the same parameters and distinct
  for different ones. Tag constants are referenced, never typed as literals at a second site.
- **Unit — serialization:** every cached result type round-trips through `System.Text.Json`
  unchanged. This is the guard against a later change caching a type that holds an aggregate.
- **Part D — the existing integration suite, unchanged.** That is the whole verification for
  10.13–10.15: bodies move to a base class, behaviour does not. A failure means a specification did
  more than a plain id match.
- **Part D — hydration:** after 10.15, assert that a finder which previously returned a bare
  aggregate now has its navigation populated, pinning the guarantee rather than leaving it
  incidental.

---

## Rollout

**10.1–10.5 ship without Redis.** `HybridCache` runs L1-only when no `IDistributedCache` is
registered, so the decorator, tag eviction, stampede protection and lookup caching are all a strict
improvement on the current code before any infrastructure exists. They may land as their own PR
ahead of the rest of this stage.

From 10.6 onward Redis is a hard runtime dependency: compose, CI, and production provisioning land
**first**.
Deploy order changes permanently: `migrate` step, then instances. First deploy after this stage
runs the Quartz schema migration.

---

## Verification

1. `dotnet build --no-incremental` — 0 warnings, 0 errors; `dotnet csharpier check .`.
2. Unit + integration green (Redis Testcontainer in the fixture).
3. `grep -rn "IMemoryCache" src/Modules/Content` → nothing (the popular feeds were its only Content
   consumers; `AddMemoryCache()` stays in `Program.cs` as `HybridCache`'s L1).
   `grep -rn "CacheInvalidator\|CancellationChangeToken" src` → nothing.
4. `grep -rn "GetAwaiter().GetResult()" src/Modules src/Shared` → no seeding/migration call sites left.
5. `docker build .` succeeds from a clean context.
6. `grep -rn "public async Task<.*> GetByIdOrThrowAsync" src/Modules` → the 13 implementations are
   gone; only the base remains.
7. Two local instances + one Redis: like on :5025, feed on :5026 reflects it within one request.

---

## SOLID gates

Acceptance criteria for Parts A and D, checkable at review.

| Principle | Gate |
| --- | --- |
| **SRP** | No `UseCases` file contains caching code; no `Cached*` type queries a `DbSet` |
| **OCP** | No handler is edited to *gain* caching — 10.2's diff removes handler lines and adds none |
| **LSP** | Only queries that tolerate staleness implement `ICacheableRequest`; a `Query()` override may add `Include` calls but never narrows a result set |
| **ISP** | `ICacheableRequest` is implemented only where a real `CacheKey` exists; after 10.17 no handler depends on a repository interface spanning more than one aggregate |
| **DIP** | `grep -rn "Caching.Memory\|Caching.Distributed\|MemoryCacheEntryOptions\|CancellationChangeToken" src/Modules/*/*/Application` returns nothing. `Microsoft.Extensions.Caching.Hybrid` is permitted — `HybridCache` is an abstract class in `Microsoft.Extensions.Caching.Abstractions`, and the event handlers legitimately name it |

---

**PRs:** `fix(infra): make the app safe to run on more than one instance` (Parts A–C) and, if the
combined diff is too large to review, `refactor(data): introduce repository contracts and a
hydration seam` (Part D).
