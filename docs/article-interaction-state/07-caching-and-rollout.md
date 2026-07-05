# Caching Correctness & Rollout

The one pitfall that can turn this feature into a data-leak bug, in depth — plus the rollout
order and open questions.

---

## The pitfall: per-user flags must never be served from a shared cache

`IsLiked` / `IsBookmarked` are **per-user**. `LikeCount` / `BookmarkCount` are global. If a
list response carrying user A's flags is cached under a **user-agnostic** key and later served
to user B, then B sees A's like/bookmark state. That is both a correctness defect (wrong
toggles) and a privacy defect (B learns what A interacted with).

### Current state: no bug today

No article read handler is cached right now (verified: `PublicGetArticleBySlugHandler`,
`PublicGetPublishedArticlesHandler`, `PublicGetPromotedArticlesHandler`,
`PublicGetArticlePromotionFeedHandler` inject no `IMemoryCache` / `IDistributedCache`). Only
the tag lookup handlers (`PublicGetAllTagsHandler`, `PublicGetPopularTagsHandler`) use
`IMemoryCache`, and tags carry no per-user data. So **shipping Phases 1–2 as written
introduces no cache bug.**

The risk is **future**: the promoted / promotion-feed lists are natural cache candidates
(small, hot, change slowly). Whoever adds that cache must not cache the flags.

---

## The anti-pattern (do not do this)

```csharp
// WRONG: the cached payload carries per-user flags under a shared key.
var dtos = await cache.GetOrCreateAsync("promoted-articles", async _ =>
{
    var articles = await articleRepository.GetPromotedAsync(ct);
    var (liked, bookmarked) = await ResolveInteractionSetsAsync(currentUserId, ids, ct);
    return await articles.ToArticleSummaryDtosAsync(mapper, fileRepository, liked, bookmarked, ct);
});
// First caller's liked/bookmarked set is now frozen in the cache and served to everyone.
```

The moment per-user state is inside the cached object under a user-agnostic key, it leaks.

---

## Strategy 1 (preferred): cache user-agnostic, layer flags after read

Cache the **all-`false`** payload (the original `ToArticleSummaryDtosAsync` with no flag
sets). After reading from cache, resolve the two id sets and re-stamp with `record with` — the
per-user data never enters the cache.

```csharp
// Cache holds user-agnostic summaries only (IsLiked = IsBookmarked = false).
IReadOnlyList<ArticleSummaryDto> baseDtos = await cache.GetOrCreateAsync(
    "promoted-articles",                 // key contains NO user id
    async _ =>
    {
        IReadOnlyList<ArticleEntity> articles = await articleRepository.GetPromotedAsync(ct);
        return await articles.ToArticleSummaryDtosAsync(mapper, fileRepository, ct);
    });

// Per request, layer the current user's flags on top of the cached payload.
if (currentUserId is Guid userId)
{
    var ids = baseDtos.Select(d => d.Id).ToList();
    HashSet<Guid> liked = await articleRepository.GetLikedArticleIdsAsync(userId, ids, ct);
    HashSet<Guid> bookmarked = await articleRepository.GetBookmarkedArticleIdsAsync(userId, ids, ct);

    return baseDtos
        .Select(d => d with { IsLiked = liked.Contains(d.Id), IsBookmarked = bookmarked.Contains(d.Id) })
        .ToList();
}

return baseDtos; // anonymous → flags already false
```

- Cache key rule: **never** include a user id in the shared list cache key.
- Records are immutable and `with` produces a fresh copy, so re-stamping cannot mutate the
  cached instance.
- Cache hit rate stays high (one shared entry); only two cheap indexed batch queries run per
  authenticated request.

This is exactly why Phase 2 keeps the original user-agnostic `ToArticleSummaryDtosAsync` and
adds the flag-stamping as a separate overload / `with` step (doc 03 §3).

## Strategy 2: skip the cache for authenticated requests

Serve anonymous traffic from the shared cache (flags all `false`, correct by definition);
bypass the cache and query directly when `CurrentUserId` is set.

```csharp
if (currentUserId is null)
{
    return await cache.GetOrCreateAsync("promoted-articles", /* user-agnostic build */);
}
// authenticated: build fresh with flags, no cache read/write
```

Simpler, but authenticated users lose the cache benefit. Acceptable if authenticated feed
traffic is a small fraction; Strategy 1 is otherwise preferred.

## Never: per-user cache keys for lists

Keying the list cache by user id (`"promoted-articles:{userId}"`) is correct but explodes the
cache (one entry per user per list) and collapses the hit rate. Reject it for lists. (It is
only reasonable for the single-article interaction-state resource in
[04-alternative-endpoint.md](04-alternative-endpoint.md), which is tiny.)

---

## Rollout order

1. **Phase 1 — detail flags.** Add fields to `ArticleDetailDto`, wire the get-by-slug
   query/handler/endpoint, reuse `HasLikedAsync` / `HasBookmarkedAsync`. Ship + test. Lowest
   risk, immediate value for the single-article page.
2. **Phase 2 — feed flags.** Add fields to `ArticleSummaryDto`, add the two batch repository
   methods, add the batch mapper overload, wire the three list handlers/endpoints. Ship +
   test. **No caching added in this step** — the handlers stay uncached, so there is no cache
   correctness surface yet.
3. **Frontend.** Regenerate the client; seed the toggle hooks from the entity flags (doc 05).
4. **Later (optional) — caching.** If/when the promoted or promotion-feed lists are cached,
   apply Strategy 1. The cross-user integration test in [06-testing.md](06-testing.md) §"No
   cross-user cache leak" is the gate that must pass before that cache ships.

Phases 1 and 2 are independent — Phase 1 can ship alone and deliver the single-article-page
fix while Phase 2 is in review.

---

## Open questions

1. **Should the flags be non-nullable in the generated client?** They default to `false`
   server-side, so they are effectively always present. Recommend the frontend map them with
   `?? false` regardless (doc 05 §3), so a stale generated client never breaks.
2. **Two batch queries or one?** Phase 2 runs a liked batch and a bookmarked batch (two
   round-trips). A single query hitting both tables (e.g. a `UNION`/`GROUP BY` returning
   per-article flags) could halve the round-trips. Not recommended for the first cut — two
   simple indexed queries are clearer and each is already O(page size). Revisit only if the
   feed p99 shows it matters.
3. **Do admin list handlers need the flags?** No. The flags are a public-reader concern.
   Admin summary call sites keep using the user-agnostic mapper (flags default `false`) and
   ignore them.
4. **Caching Phase-2 lists — who owns it?** Deferred. When taken up, Strategy 1 + the
   cross-user leak test are mandatory.
