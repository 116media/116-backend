# 16 — Caching architecture

A focused review of how results are cached, prompted by the reference-codebase study in
[`docs/repository-and-caching`](../repository-and-caching/00-overview.md). Doc 04 §4.8 identified
the multi-instance symptom; this doc covers the shape of the caching code itself, which that
finding did not examine, and **supersedes 04 §4.8's proposed remedy** (see §16.4).

---

## What exists today

Four public query handlers each carry a private copy of the same read-through block:

| Handler | Key |
| --- | --- |
| `PublicGetPopularArticlesHandler` | `popular_articles_{limit}_{category}_{exclude}` |
| `PublicGetPopularVideosHandler` | `popular_videos_{limit}_{category}_{exclude}` |
| `PublicGetPopularTagsHandler` | `popular_tags_{limit}_{contentType}` |
| `PublicGetAllTagsHandler` | `all_tags_{limit}_{contentType}` |

Invalidation is the part already done well: `ICacheInvalidator` + three region markers, three
domain-event handlers, dispatched by `DispatchDomainEventsInterceptor.SavedChangesAsync` — **after**
the commit succeeds and discarded when it fails. Neither reference codebase has commit-safe
invalidation. That timing is correct and must survive any rewrite.

---

## 16.1 The read-through block is duplicated four times, and has already diverged

**Severity: Medium**

**Where:** `PublicGetPopularArticlesHandler.cs:47-73` and the three siblings — each owns a key
format, a `CacheTtl` constant, an `IMemoryCache` dependency, a `TryGetValue`/`Set` pair and an
`AddExpirationToken` call.

**Problem/why.** A change to caching strategy is a four-file edit with four chances to diverge, and
it already has: `PublicGetAllTagsHandler` added a `cacheable` guard that skips storage when a search
term is present — free-text would otherwise produce an unbounded key space — and the other three
have no equivalent concept. Nothing structural prevents a fifth handler from omitting the eviction
token entirely and caching a feed that never invalidates.

**Solution.** One decorator over `IRequestHandler<,>`, joining the three already registered in
`CqrsExtension.cs:62-64`. The query record declares its own key, TTL and tags; the guard becomes a
property on the query rather than a branch in a handler.

---

## 16.2 There is no stampede protection

**Severity: Medium** · not previously reported.

**Where:** all four handlers — `TryGetValue` → miss → repository query → `Set`, with no coordination
between concurrent callers.

**Problem/why.** On expiry under load, every concurrent request for the same key runs the ranking
query. The popular-articles query is the most expensive read in Content (weighted engagement scoring
over the published set); the moment its entry expires is exactly the moment traffic is heaviest, so
the cost of a miss is multiplied by concurrency rather than paid once. This is invisible in testing
and appears only under production traffic.

**Solution.** `HybridCache.GetOrCreateAsync` coordinates concurrent misses on one key so the factory
runs once. No hand-written locking.

---

## 16.3 The most cacheable data in the module is not cached at all

**Severity: Medium** · overlaps `[04 §4.11]` (repository shape), distinct finding.

**Where:** `LookupRepository.GetAllContentTypesAsync`, `GetActiveContentTypesAsync`,
`GetAllPricingTiersAsync`, `GetAllPromotionLevelsAsync`, `GetActivePromotionLevelsAsync`, plus the
category reads — every one hits Postgres on every request.

**Problem/why.** These are tens of rows, mutated by an admin a handful of times a year, and read on
effectively every page. They are the textbook reference-data case — the `ICacheable` shape the
`dotnet-microservices` reference reserves for exactly this — and they carry no cache while the four
expensive-but-rare feeds do.

**Solution.** The same marker interface, with a `content:lookups` tag and a longer TTL; admin
mutation handlers evict the tag. No new mechanism.

---

## 16.4 The cache technology is welded into the use cases

**Severity: Medium** · **supersedes the remedy in `[04 §4.8]`**.

**Where:** four handlers depend on `IMemoryCache`, `MemoryCacheEntryOptions` and
`CancellationChangeToken`.

**Problem/why.** Not a broken dependency direction — those types live in
`Microsoft.Extensions.Caching.Abstractions`, so the Application layer depends on an interface, not a
store. The defect is narrower and real: that interface is in-process **by construction**.
`TryGetValue` is synchronous and eviction is expressed as a `CancellationChangeToken`, a
process-local primitive with no distributed equivalent. Because the handler states *how* the cache
works rather than *what* it needs cached, moving to Redis is not a registration change — it rewrites
four handlers and replaces the eviction model.

**Solution — and why it differs from `[04 §4.8]`.** That finding proposed
`AddStackExchangeRedisCache` plus a Redis version key (`INCR popular_articles_version`) folded into
each handler's cache key. That works, but it is a hand-built version of something the framework now
ships, and it leaves the key composition inside the handlers — the actual defect.

Use `HybridCache` (`Microsoft.Extensions.Caching.Hybrid`) instead:

| `[04 §4.8]` remedy | `HybridCache` |
| --- | --- |
| `IDistributedCache` (Redis only) | L1 memory + L2 Redis behind one API |
| hand-written version key per region | `RemoveByTagAsync(tag)` |
| version composed into keys inside each handler | key declared by the query record |
| no stampede protection | built in |
| L2 required to ship | L1-only when no `IDistributedCache` is registered |

The last row matters for sequencing: the rewrite lands and improves matters **before** Redis exists,
and Redis then adds L2 with one registration and no code change. `[04 §4.8]`'s job-clustering half
is unaffected and stands as written.

---

## What is deliberately not cached

Recorded so a later reader does not treat the omissions as oversights.

| Read | Why not |
| --- | --- |
| Anything carrying `IsLiked` / `IsBookmarked` | Per-user; the key space is users × articles and the value is wrong for everyone else |
| Free-text search on any list | Unbounded key space |
| Admin paginated lists | Low hit rate, high staleness cost, and the reader is usually the person who just wrote |
| Article and video detail pages | Engagement counters move continuously; the entry would thrash |
| Anything returning an aggregate | It would have to serialize for L2 — the constraint that forced the EShop reference into two hand-written `JsonConverter`s and reflection into a private field |

---

## A note on where `HybridCache` may be named

`HybridCache` is an **abstract class in `Microsoft.Extensions.Caching.Abstractions`**, not an
implementation — the same category `IMemoryCache` occupies today. Application-layer code may name
it: the three cache event handlers inject it to call `RemoveByTagAsync`, and the decorator lives in
`Shared.Application` alongside the three it joins.

What must not appear in an application layer after this stage: `MemoryCacheEntryOptions`,
`CancellationChangeToken`, `IDistributedCache`, or any Redis type. Those are store-specific and are
exactly what §16.4 is about.

## Sequencing

All of it is **[Stage 10 — Caching, concurrency and data access]**, Part A (checklist 10.1–10.6).
The decorator, the marker interface and the tag constants have no infrastructure dependency and may
be pulled ahead of the Redis work in the same stage; only the L2 registration (10.6) and the
two-instance coherence test need Redis.

Stage 10's Part A also carries the **full candidate census** (10.5): every read path in the solution
classified into cacheable groups and a never-cache list. 4 queries are cached today; 26 should be.
Two findings this doc did not have room for surfaced there — the video and artist feeds carry no
user identity and were never cached, and twelve feeds carry a nullable `CurrentUserId` used only to
stamp interaction flags, so their anonymous projection is cacheable as-is.

Full design, code and migration order:
[`docs/repository-and-caching/04-target-design.md`](../repository-and-caching/04-target-design.md)
and [`05-migration-plan.md`](../repository-and-caching/05-migration-plan.md).
