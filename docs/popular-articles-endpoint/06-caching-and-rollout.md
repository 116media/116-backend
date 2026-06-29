# Caching, Invalidation, Performance, Rollout, and Open Questions

---

## Caching design

The popular-articles cache is a 1:1 structural copy of the popular-tags cache:

- **Store:** `IMemoryCache` (in-process). Same as popular-tags. No distributed cache — the
  API is a single logical process today, and eviction-by-token is an in-process construct.
- **TTL:** `TimeSpan.FromMinutes(10)` absolute expiration. Same as popular-tags. Popularity
  moves slowly; a 10-minute worst-case staleness (when no mutation happens to bust it sooner)
  is acceptable for a sidebar.
- **Key:** `popular_articles_{limit}_{categoryId|all}_{excludeId|none}`. Each distinct
  argument combination is its own entry.
- **Eviction token:** every entry registers `new CancellationChangeToken(invalidator.GetEvictionToken())`.
  One `Invalidate()` cancels the token and evicts every combination at once.

### Cache-key cardinality and the `excludeId` optimization

Including `excludeId` in the key is correct but multiplies entries by the number of distinct
currently-viewed articles. In practice the sidebar uses a single fixed `limit` and a small set
of categories, so the dominant multiplier is `excludeId`. Two mitigations, in order of
preference for scale:

1. **Ship as specified** (key includes `excludeId`). Simplest; fine for launch volumes.
   Entries are small (≤ `limit` DTOs) and expire in 10 min.
2. **Drop `excludeId` from the key** (recommended once article traffic grows): cache the
   unfiltered top `limit + 1` per `(limit, categoryId)`, then remove `excludeId` and slice to
   `limit` in the handler after the cache read. Cardinality collapses to `(limit, categoryId)`.
   Cost: one extra DB row and a trivial in-memory filter. Requesting `limit + 1` guarantees a
   full `limit` remains even after dropping the excluded article.

Both are documented so the team can pick per observed cardinality; the specs implement (1).

---

## Invalidation — which mutations bust the cache

The ranking is a function of the four engagement counters **and** of publish state (only
`Published` articles are ranked). Any mutation that changes either must call
`cacheInvalidator.Invalidate()` **after** `CommitAsync`, exactly like the tag handlers.

### Engagement mutations (change a counter → change score)

| Handler | Method |
|---------|--------|
| `PublicLikeArticleHandler` | `IncrementLikeCount` |
| `PublicUnlikeArticleHandler` | `DecrementLikeCount` |
| `PublicAddArticleCommentHandler` | `IncrementCommentCount` |
| `PublicDeleteArticleCommentHandler` | `DecrementCommentCount` |
| `PublicShareArticleHandler` | `IncrementShareCount` |
| `PublicBookmarkArticleHandler` | `IncrementBookmarkCount` |
| `PublicUnbookmarkArticleHandler` | `DecrementBookmarkCount` |

### Membership mutations (change the candidate set)

| Handler | Effect on ranking |
|---------|-------------------|
| `AdminPublishArticleHandler` | a new article enters the ranked set |
| `AdminArchiveArticleHandler` | a published article leaves the ranked set |
| Any unpublish / hard-delete of a published article | leaves the ranked set |

Category re-assignment of a published article also affects `categoryId`-scoped entries; if an
edit path can move a published article between categories, invalidate there too.

### Invalidation frequency trade-off

Likes/comments/shares/bookmarks are high-frequency events on a busy site. Calling
`Invalidate()` on every one means the cache is busted often, approaching pass-through under
heavy engagement. That is **acceptable and correct** (same behavior the tag cache accepts on
tag edits) — the cache still absorbs read bursts between writes, and each miss is one cheap
top-N query. If engagement volume ever makes this a problem, the mitigation is **not** to skip
invalidation (that serves stale rankings) but to **debounce** it: coalesce invalidations into
a short window (e.g. evict at most once per 30s) via a timer in the invalidator implementation.
This is noted as an open question below; v1 invalidates eagerly like popular-tags.

---

## Performance and indexing

The ranking query is:

```sql
SELECT ... FROM content.articles a
WHERE a.status = 'Published'            -- always
  [AND a.category_id = @categoryId]     -- optional
  [AND a.id <> @excludeId]              -- optional
ORDER BY (4*a.like_count + 3*a.comment_count + 2*a.share_count + 1*a.bookmark_count) DESC,
         a.published_at DESC
LIMIT @limit;
```

- The `ORDER BY` is over a **computed expression**, so a plain B-tree index on individual
  counter columns does **not** serve the sort. Options:
  1. **Do nothing for v1.** The published-article count is modest and the query is
    `WHERE status = 'Published'` + a sort + `LIMIT`. Postgres filters then sorts; at current
    data volumes this is cheap, and the 10-minute cache means it runs rarely.
  2. **Partial expression index** (future, if the plan shows a costly sort):
    ```sql
    CREATE INDEX ix_articles_popularity
      ON content.articles ((4*like_count + 3*comment_count + 2*share_count + bookmark_count) DESC,
                           published_at DESC)
      WHERE status = 'Published';
    ```
    The partial predicate (`WHERE status = 'Published'`) keeps the index small and aligned
    with the always-applied filter. **Caveat:** the expression and weights are baked into the
    index — retuning the weights requires rebuilding it. Because the weights are meant to be
    tunable, adopt this index only once the weights have stabilized. Until then, rely on the
    cache + the modest row count.
  3. A supporting index on `(status, category_id)` helps the category-scoped filter regardless
    of the sort strategy.
- If option (2) is adopted, it is a standard EF migration on `ContentDbContext`:
  ```bash
  dotnet ef migrations add AddArticlePopularityIndex \
    --project src/Modules/Content/Content/Infrastructure \
    --startup-project src/Api \
    --context ContentDbContext
  ```
  The raw `CREATE INDEX ... ((expr))` goes in the migration's `Up` via `migrationBuilder.Sql(...)`
  (expression + partial indexes are not expressible through the fluent `HasIndex` API).

No new columns and **no migration are required for v1** — the endpoint reads existing columns.

---

## Rollout

1. **Backend, additive.** New slice, new builder, new repo method, new invalidator. No schema
   change, no change to existing endpoints. The published/promoted endpoints keep working.
2. **Ship behind the existing anonymous public surface** — it is a read endpoint with
   `ContentBrowsing` rate limiting, safe to expose immediately.
3. **Frontend swaps the sidebar source** (`04-frontend-integration.md`) after the backend is
   live and `yarn api:generate` has run. Because the sidebar already renders
   `ArticleSummaryDto`, the swap is source-only — no UI change, easily reversible by pointing
   the hook back at `getPromotedArticlesUseCase`.
4. **Empty-state consideration:** on a fresh deployment with no engagement, every score is 0
   and the endpoint ranks purely by `PublishedAt` desc — effectively recency, a sensible
   default. If the product wants promoted articles as a cold-start fallback, keep that
   fallback in the hook (documented in `04`).

---

## Open questions

1. **Trending vs all-time.** v1 is all-time weighted engagement. A "trending" variant needs a
   recency signal — either a freshness window (`WHERE published_at >= now() - interval`, cheap)
   or a time-decay factor (Hacker-News gravity, needs a decay expression in the sort and is
   hard to index). Best deferred until there is a product need for trending.
2. **Weight tuning.** The `4/3/2/1` weights (like/comment/share/bookmark) are ordinal
   guesses, not fitted. Once there is
   engagement data, the weights should be revisited — possibly A/B tested. They are isolated
   in `PopularArticlesScoring` for exactly this.
3. **Invalidation debounce.** Eager `Invalidate()` on every engagement event is correct but
   may approach cache pass-through under heavy load. If observed, add a debounce/coalesce
   window to the invalidator rather than dropping invalidations.
4. **Personalization.** This endpoint is global (same ranking for everyone) and cacheable
   precisely because it is not per-user. Personalized "popular for you" would require a
   different, per-user, uncacheable design and is explicitly out of scope.
5. **`excludeId` cache-key strategy.** Whether to include `excludeId` in the cache key (simple,
   higher cardinality) or drop it and post-filter (lower cardinality) depends on observed
   article traffic. See the caching section above.
